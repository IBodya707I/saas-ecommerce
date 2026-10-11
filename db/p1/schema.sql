-- База даних: ordering_db (PostgreSQL)

CREATE TABLE IF NOT EXISTS customers (
    id            UUID PRIMARY KEY,                          
    store_id      UUID NOT NULL,                           
    phone         VARCHAR(30) NOT NULL,
    email         VARCHAR(256) NULL,
    full_name     VARCHAR(150) NOT NULL,
    password_hash VARCHAR(256) NULL,      
    
    is_registered BOOLEAN NOT NULL DEFAULT false,
    version       INT NOT NULL DEFAULT 1,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by    VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at    TIMESTAMPTZ NULL,
    updated_by    VARCHAR(100) NULL,
    is_deleted    BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT ck_customers_phone CHECK (phone ~ '^\+[1-9][0-9]{7,14}$'),
    CONSTRAINT ck_customers_registered_auth CHECK (NOT is_registered OR password_hash IS NOT NULL),
    CONSTRAINT uq_customers_store_id UNIQUE (store_id, id)
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_customers_store_phone_active ON customers (store_id, phone) WHERE is_deleted = false;
CREATE UNIQUE INDEX IF NOT EXISTS uq_customers_store_email_active ON customers (store_id, lower(email)) WHERE is_deleted = false AND email IS NOT NULL;

CREATE TABLE IF NOT EXISTS orders (
    id             UUID PRIMARY KEY,
    customer_id    UUID NOT NULL,
    store_id       UUID NOT NULL,
    status         VARCHAR(30) NOT NULL DEFAULT 'Submitted',
    payment_method VARCHAR(30) NOT NULL DEFAULT 'Prepaid',
    total_amount   NUMERIC(18,2) NOT NULL DEFAULT 0.00,
    version        INT NOT NULL DEFAULT 1,

    created_at     TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by     VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at     TIMESTAMPTZ NULL,
    updated_by     VARCHAR(100) NULL,
    is_deleted     BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT fk_orders_customer FOREIGN KEY (store_id, customer_id) REFERENCES customers(store_id, id) ON DELETE RESTRICT,
    CONSTRAINT uq_orders_store_id UNIQUE (store_id, id),
    CONSTRAINT ck_orders_total_amount CHECK (total_amount >= 0),
    CONSTRAINT ck_orders_status CHECK (status IN ('Submitted', 'Paid', 'Shipped', 'Cancelled')),
    CONSTRAINT ck_orders_payment_method CHECK (payment_method IN ('Prepaid', 'CashOnDelivery'))
);

CREATE TABLE IF NOT EXISTS order_details (
    order_id          UUID PRIMARY KEY,
    delivery_provider VARCHAR(50) NOT NULL,
    city              VARCHAR(100) NOT NULL,
    warehouse_address VARCHAR(250) NOT NULL,
    recipient_phone   VARCHAR(30) NOT NULL,
    notes             VARCHAR(500) NULL,

    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by        VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at        TIMESTAMPTZ NULL,
    updated_by        VARCHAR(100) NULL,
    is_deleted        BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT fk_order_details_orders FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    CONSTRAINT ck_order_details_recipient_phone CHECK (recipient_phone ~ '^\+[1-9][0-9]{7,14}$'),
    CONSTRAINT ck_order_details_provider CHECK (delivery_provider IN ('NovaPoshta', 'UkrPoshta'))
);

CREATE TABLE IF NOT EXISTS products_local (
    id                UUID PRIMARY KEY,
    store_id          UUID NOT NULL,
    name              VARCHAR(200) NOT NULL,
    price             NUMERIC(18,2) NOT NULL,
    stock             INT NOT NULL DEFAULT 0,
    is_active         BOOLEAN NOT NULL DEFAULT true,

    source_updated_at TIMESTAMPTZ NULL,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by        VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at        TIMESTAMPTZ NULL,
    updated_by        VARCHAR(100) NULL,
    is_deleted        BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT uq_products_local_store_id UNIQUE (store_id, id),
    CONSTRAINT ck_products_local_price CHECK (price > 0),
    CONSTRAINT ck_products_local_stock CHECK (stock >= 0)
);

CREATE TABLE IF NOT EXISTS order_items (
    order_id     UUID NOT NULL,
    store_id     UUID NOT NULL,
    product_id   UUID NOT NULL,
    product_name VARCHAR(200) NOT NULL,
    unit_price   NUMERIC(18,2) NOT NULL,
    quantity     INT NOT NULL,

    created_at   TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by   VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at   TIMESTAMPTZ NULL,
    updated_by   VARCHAR(100) NULL,
    is_deleted   BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT pk_order_items PRIMARY KEY (order_id, product_id),
    CONSTRAINT fk_order_items_orders FOREIGN KEY (store_id, order_id) REFERENCES orders(store_id, id) ON DELETE CASCADE,
    CONSTRAINT fk_order_items_products_local FOREIGN KEY (store_id, product_id) REFERENCES products_local(store_id, id) ON DELETE RESTRICT,
    CONSTRAINT ck_order_items_unit_price CHECK (unit_price > 0),
    CONSTRAINT ck_order_items_quantity CHECK (quantity > 0)
);

CREATE TABLE IF NOT EXISTS order_status_history (
    id         BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id   UUID NOT NULL,
    old_status VARCHAR(30) NULL,
    new_status VARCHAR(30) NOT NULL,
    changed_by VARCHAR(100) NOT NULL,
    changed_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    comment    VARCHAR(500) NULL,

    CONSTRAINT fk_order_status_history_orders FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    CONSTRAINT ck_order_status_history_old CHECK (old_status IS NULL OR old_status IN ('Submitted', 'Paid', 'Shipped', 'Cancelled')),
    CONSTRAINT ck_order_status_history_new CHECK (new_status IN ('Submitted', 'Paid', 'Shipped', 'Cancelled'))
);

-- ІНДЕКСИ ПІД КОНКРЕТНІ ЗАПИТИ (ORDERING DB)
-- Кожен індекс суворо оптимізує визначений бізнес-запит

-- Запит 1: Отримання історії замовлень покупця в особистому кабінеті (Index-Only Scan)
-- Запит: SELECT id, status, total_amount, created_at FROM orders WHERE customer_id = $1 AND is_deleted = false ORDER BY created_at DESC;
CREATE INDEX IF NOT EXISTS ix_orders_customer_active 
ON orders (customer_id, created_at DESC) 
INCLUDE (id, status, total_amount) 
WHERE is_deleted = false;

-- Запит 2: Вибірка нових або оброблюваних замовлень магазину за статусом (Index-Only Scan для бекофісу)
-- Запит: SELECT id, total_amount, created_at FROM orders WHERE store_id = $1 AND status = $2 AND is_deleted = false ORDER BY created_at DESC;
CREATE INDEX IF NOT EXISTS ix_orders_store_status 
ON orders (store_id, status, created_at DESC) 
INCLUDE (id, total_amount) 
WHERE is_deleted = false;

-- Запит 3: Загальний список замовлень магазину без фільтра за статусом (Головний дашборд замовлень)
-- Запит: SELECT id, status, total_amount, created_at FROM orders WHERE store_id = $1 AND is_deleted = false ORDER BY created_at DESC;
CREATE INDEX IF NOT EXISTS ix_orders_store_created 
ON orders (store_id, created_at DESC) 
INCLUDE (id, status, total_amount) 
WHERE is_deleted = false;

-- Запит 4: Швидка перевірка залишків та цін активних товарів перед резервуванням у кошику (Index-Only Scan)
-- Запит: SELECT id, price, stock FROM products_local WHERE store_id = $1 AND is_active = true AND is_deleted = false;
CREATE INDEX IF NOT EXISTS ix_products_local_store_active 
ON products_local (store_id, is_active) 
INCLUDE (id, price, stock) 
WHERE is_deleted = false;

-- Запит 5: Виведення хронологічного таймлайну зміни статусів замовлення
-- Запит: SELECT old_status, new_status, changed_by, changed_at, comment FROM order_status_history WHERE order_id = $1 ORDER BY changed_at ASC, id ASC;
CREATE INDEX IF NOT EXISTS ix_order_status_history_order_timeline 
ON order_status_history (order_id, changed_at ASC, id ASC);

-- Запит 6: Зворотний пошук замовлень за товаром для формування аналітики продажів
-- Запит: SELECT order_id, quantity, unit_price FROM order_items WHERE product_id = $1 AND is_deleted = false;
CREATE INDEX IF NOT EXISTS ix_order_items_product_id 
ON order_items (product_id) 
WHERE is_deleted = false;