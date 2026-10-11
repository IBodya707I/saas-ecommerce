-- Збережувані процедури для ordering_db

-- ТРАНЗАКЦІЙНІСТЬ ТА АТОМАРНІСТЬ:
-- У PostgreSQL виклик CALL виконується в межах неявної транзакції. 
-- Будь-який необроблений RAISE EXCEPTION автоматично відкочує всі зміни (ROLLBACK). 
-- Конкурентний доступ захищається явним блокуванням SELECT ... FOR UPDATE.

-- МАПІНГ КОДІВ ПОМИЛОК НА HTTP-СТАТУСИ:
-- ----------------------------------------------------------------------------
-- Код БД | HTTP-статус       | Опис ситуації
-- -------|-------------------|------------------------------------------------
-- ORD00  | 400 Bad Request   | Пустий кошик або відсутні обов'язкові дані
-- ORD01  | 400 Bad Request   | Недопустимий перехід статусів State Machine
-- ORD02  | 404 Not Found     | Сутність не знайдена (клієнт, товар, замовлення)
-- ORD03  | 400 Bad Request   | Помилка валідації формату (телефон, провайдер)
-- ORD04  | 400 Bad Request   | Спроба відвантажити неоплачене Prepaid замовлення
-- STK01  | 409 Conflict      | Недостатньо залишків (stock) на складі
-- OPT01  | 409 Conflict      | Конфлікт версій (Optimistic Concurrency Control)
-- ============================================================================


-- 1. СТВОРЕННЯ ЗАМОВЛЕННЯ З ПЕРЕВІРКОЮ ТА СПИСАННЯМ ЗАЛИШКІВ 

CREATE OR REPLACE PROCEDURE usp_create_order(
    p_order_id          UUID,
    p_customer_id       UUID,
    p_store_id          UUID,
    p_delivery_provider VARCHAR(50),
    p_city              VARCHAR(100),
    p_warehouse_address VARCHAR(250),
    p_recipient_phone   VARCHAR(30),
    p_notes             VARCHAR(500),
    p_items             JSONB,
    p_payment_method    VARCHAR(30) DEFAULT 'Prepaid'
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_total_amount    NUMERIC(18,2) := 0.00;
    v_item            RECORD;
    v_current_stock   INT;
    v_is_active       BOOLEAN;
    v_product_name    VARCHAR(200);
    v_unit_price      NUMERIC(18,2);
BEGIN
    -- Валідація пустого кошика
    IF p_items IS NULL OR jsonb_array_length(p_items) = 0 THEN
        RAISE EXCEPTION 'Cart cannot be empty' USING ERRCODE = 'ORD00';
    END IF;

    -- Валідація вхідних даних
    IF p_delivery_provider NOT IN ('NovaPoshta', 'UkrPoshta') THEN
        RAISE EXCEPTION 'Unsupported delivery provider: %', p_delivery_provider USING ERRCODE = 'ORD03';
    END IF;

    IF p_recipient_phone !~ '^\+[1-9][0-9]{7,14}$' THEN
        RAISE EXCEPTION 'Invalid phone format: %', p_recipient_phone USING ERRCODE = 'ORD03';
    END IF;

    -- Перевірка існування клієнта
    IF NOT EXISTS (
        SELECT 1 FROM customers 
        WHERE store_id = p_store_id AND id = p_customer_id AND is_deleted = false
    ) THEN
        RAISE EXCEPTION 'Customer % not found in store %', p_customer_id, p_store_id USING ERRCODE = 'ORD02';
    END IF;

    -- Створення заголовка замовлення
    INSERT INTO orders (
        id, customer_id, store_id, status, payment_method, total_amount, version, created_by
    )
    VALUES (
        p_order_id, p_customer_id, p_store_id, 'Submitted', p_payment_method, 0.00, 1, 'usp_create_order'
    );

    -- Додавання логістичних деталей
    INSERT INTO order_details (
        order_id, delivery_provider, city, warehouse_address, recipient_phone, notes, created_by
    )
    VALUES (
        p_order_id, p_delivery_provider, p_city, p_warehouse_address, p_recipient_phone, p_notes, 'usp_create_order'
    );

    -- Обробка позицій кошика (Групування захищає від дублікатів у JSON)
    FOR v_item IN 
        SELECT product_id, SUM(quantity)::INT as quantity 
        FROM jsonb_to_recordset(p_items) AS x(product_id UUID, quantity INT)
        GROUP BY product_id
    LOOP
        IF v_item.quantity IS NULL OR v_item.quantity <= 0 THEN
            RAISE EXCEPTION 'Quantity must be positive integer for product %', v_item.product_id USING ERRCODE = 'ORD03';
        END IF;

        -- FOR UPDATE блокує товар від паралельних транзакцій
        SELECT stock, is_active, name, price 
        INTO v_current_stock, v_is_active, v_product_name, v_unit_price
        FROM products_local
        WHERE store_id = p_store_id AND id = v_item.product_id AND is_deleted = false
        FOR UPDATE;

        IF NOT FOUND THEN
            RAISE EXCEPTION 'Product % not found in catalog replica', v_item.product_id USING ERRCODE = 'ORD02';
        END IF;

        IF NOT v_is_active THEN
            RAISE EXCEPTION 'Product % is deactivated and cannot be ordered', v_item.product_id USING ERRCODE = 'ORD03';
        END IF;

        -- Перевірка залишків
        IF v_current_stock < v_item.quantity THEN
            RAISE EXCEPTION 'Insufficient stock for product %. Available: %, Requested: %', 
                v_product_name, v_current_stock, v_item.quantity USING ERRCODE = 'STK01';
        END IF;

        -- Списання залишку
        UPDATE products_local
        SET stock = stock - v_item.quantity,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = 'usp_create_order'
        WHERE store_id = p_store_id AND id = v_item.product_id;

        -- Запис історичного знімка (Snapshot) ціни та назви
        INSERT INTO order_items (
            order_id, store_id, product_id, product_name, unit_price, quantity, created_by
        )
        VALUES (
            p_order_id, p_store_id, v_item.product_id, v_product_name, v_unit_price, v_item.quantity, 'usp_create_order'
        );

        v_total_amount := v_total_amount + (v_unit_price * v_item.quantity);
    END LOOP;

    -- Оновлення суми
    UPDATE orders SET total_amount = v_total_amount WHERE id = p_order_id;

    -- Запис в аудит
    INSERT INTO order_status_history (order_id, old_status, new_status, changed_by, comment)
    VALUES (p_order_id, NULL, 'Submitted', 'usp_create_order', 'Order created with stock reservation');
END;
$$;


-- 2. ЗМІНА СТАТУСУ ЗАМОВЛЕННЯ (State Machine)

CREATE OR REPLACE PROCEDURE usp_change_order_status(
    p_order_id       UUID,
    p_store_id       UUID,
    p_expected_ver   INT,
    p_new_status     VARCHAR(30),
    p_changed_by     VARCHAR(100),
    p_comment        VARCHAR(500)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_curr_status    VARCHAR(30);
    v_curr_version   INT;
    v_pay_method     VARCHAR(30);
BEGIN
    SELECT status, version, payment_method
    INTO v_curr_status, v_curr_version, v_pay_method
    FROM orders
    WHERE id = p_order_id AND store_id = p_store_id AND is_deleted = false
    FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Order % not found', p_order_id USING ERRCODE = 'ORD02';
    END IF;

    -- Оптимістичне блокування
    IF v_curr_version <> p_expected_ver THEN
        RAISE EXCEPTION 'Concurrency conflict for order %. Current version: %, expected: %', 
            p_order_id, v_curr_version, p_expected_ver USING ERRCODE = 'OPT01';
    END IF;

    IF v_curr_status = p_new_status THEN
        RETURN;
    END IF;

    IF v_curr_status IN ('Cancelled') THEN
        RAISE EXCEPTION 'Cannot change status from terminal state: %', v_curr_status USING ERRCODE = 'ORD01';
    END IF;

    -- Логіка відвантаження (COD дозволяє відправку без статусу Paid, Prepaid - ні)
    IF p_new_status = 'Shipped' AND v_curr_status = 'Submitted' AND v_pay_method = 'Prepaid' THEN
        RAISE EXCEPTION 'Cannot ship unpaid prepaid order %', p_order_id USING ERRCODE = 'ORD04';
    END IF;

    -- Допустимі переходи (Враховано COD)
    IF NOT (
        (v_curr_status = 'Submitted' AND p_new_status IN ('Paid', 'Shipped', 'Cancelled')) OR
        (v_curr_status = 'Paid'      AND p_new_status = 'Shipped') OR
        (v_curr_status = 'Shipped'   AND p_new_status = 'Paid' AND v_pay_method = 'CashOnDelivery')
    ) THEN
        RAISE EXCEPTION 'Invalid status transition from % to %', v_curr_status, p_new_status USING ERRCODE = 'ORD01';
    END IF;

    UPDATE orders
    SET status = p_new_status,
        version = version + 1,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = p_changed_by
    WHERE id = p_order_id;

    INSERT INTO order_status_history (order_id, old_status, new_status, changed_by, comment)
    VALUES (p_order_id, v_curr_status, p_new_status, p_changed_by, p_comment);

    -- Автоматичне повернення залишків на склад при скасуванні
    IF p_new_status = 'Cancelled' THEN
        UPDATE products_local p
        SET stock = p.stock + oi.quantity,
            updated_at = CURRENT_TIMESTAMP,
            updated_by = 'usp_change_order_status_cancel'
        FROM order_items oi
        WHERE oi.order_id = p_order_id AND p.store_id = oi.store_id AND p.id = oi.product_id;
    END IF;
END;
$$;


-- 3. КОРЕГУВАННЯ ЗАЛИШКІВ (ІНВЕНТАРИЗАЦІЯ)

CREATE OR REPLACE PROCEDURE usp_adjust_stock(
    p_store_id   UUID,
    p_product_id UUID,
    p_delta      INT,
    p_reason     VARCHAR(100)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_curr_stock INT;
BEGIN
    SELECT stock INTO v_curr_stock
    FROM products_local
    WHERE store_id = p_store_id AND id = p_product_id AND is_deleted = false
    FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Product % not found in store %', p_product_id, p_store_id USING ERRCODE = 'ORD02';
    END IF;

    IF v_curr_stock + p_delta < 0 THEN
        RAISE EXCEPTION 'Negative stock not allowed. Product %, current %, delta %', 
            p_product_id, v_curr_stock, p_delta USING ERRCODE = 'STK01';
    END IF;

    UPDATE products_local
    SET stock = stock + p_delta,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = 'usp_adjust_stock: ' || p_reason
    WHERE store_id = p_store_id AND id = p_product_id;
END;
$$;


-- 4. СИНХРОНІЗАЦІЯ РЕПЛІКИ ТОВАРУ З CATALOG DB

CREATE OR REPLACE PROCEDURE usp_sync_product(
    p_id                UUID,
    p_store_id          UUID,
    p_name              VARCHAR(200),
    p_price             NUMERIC(18,2),
    p_is_active         BOOLEAN,
    p_source_updated_at TIMESTAMPTZ
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_source_updated_at IS NULL THEN
        RAISE EXCEPTION 'source_updated_at is strictly required for sync replication' USING ERRCODE = 'ORD03';
    END IF;

    INSERT INTO products_local (
        id, store_id, name, price, stock, is_active, source_updated_at, created_by
    )
    VALUES (
        p_id, p_store_id, p_name, p_price, 0, p_is_active, p_source_updated_at, 'replication'
    )
    ON CONFLICT (id) DO UPDATE
    SET name              = EXCLUDED.name,
        price             = EXCLUDED.price,
        is_active         = EXCLUDED.is_active,
        source_updated_at = EXCLUDED.source_updated_at,
        updated_at        = CURRENT_TIMESTAMP,
        updated_by        = 'replication'
    WHERE (products_local.source_updated_at IS NULL OR products_local.source_updated_at < EXCLUDED.source_updated_at)
      AND products_local.store_id = EXCLUDED.store_id; -- Захист від підміни магазину
END;
$$;


-- 5. СТВОРЕННЯ КЛІЄНТА

CREATE OR REPLACE PROCEDURE usp_create_customer(
    p_id            UUID,
    p_store_id      UUID,
    p_phone         VARCHAR(30),
    p_email         VARCHAR(256),
    p_full_name     VARCHAR(150),
    p_password_hash VARCHAR(256),
    p_is_registered BOOLEAN
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_phone !~ '^\+[1-9][0-9]{7,14}$' THEN
        RAISE EXCEPTION 'Invalid phone format: %', p_phone USING ERRCODE = 'ORD03';
    END IF;

    INSERT INTO customers (
        id, store_id, phone, email, full_name, password_hash, is_registered, version, created_by
    )
    VALUES (
        p_id, p_store_id, p_phone, lower(p_email), p_full_name, p_password_hash, p_is_registered, 1, 'usp_create_customer'
    );
END;
$$;


-- 6. ОНОВЛЕННЯ ДАНИХ КЛІЄНТА

CREATE OR REPLACE PROCEDURE usp_update_customer(
    p_id            UUID,
    p_store_id      UUID,
    p_expected_ver  INT,
    p_phone         VARCHAR(30),
    p_email         VARCHAR(256),
    p_full_name     VARCHAR(150)
)
LANGUAGE plpgsql
AS $$
DECLARE
    v_curr_version INT;
BEGIN
    SELECT version INTO v_curr_version
    FROM customers
    WHERE id = p_id AND store_id = p_store_id AND is_deleted = false
    FOR UPDATE;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'Customer % not found', p_id USING ERRCODE = 'ORD02';
    END IF;

    IF v_curr_version <> p_expected_ver THEN
        RAISE EXCEPTION 'Concurrency conflict for customer %. Current version: %', p_id, v_curr_version USING ERRCODE = 'OPT01';
    END IF;

    IF p_phone !~ '^\+[1-9][0-9]{7,14}$' THEN
        RAISE EXCEPTION 'Invalid phone format: %', p_phone USING ERRCODE = 'ORD03';
    END IF;

    UPDATE customers
    SET phone = p_phone,
        email = lower(p_email),
        full_name = p_full_name,
        version = version + 1,
        updated_at = CURRENT_TIMESTAMP,
        updated_by = 'usp_update_customer'
    WHERE id = p_id;
END;
$$;