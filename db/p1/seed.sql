-- Демонстраційні дані для ordering_db
-- 
-- СИНХРОНІЗАЦІЯ МАГАЗИНІВ ТА ТОВАРІВ (відповідно до catalog_db / p2):
-- Store 1: '11111111-1111-1111-1111-111111111111' - "Магія Парфумів"
-- Store 2: '22222222-2222-2222-2222-222222222222' - "TechStore Pro"

-- 1. ПОКУПЦІ (CUSTOMERS)
INSERT INTO customers (
    id, store_id, phone, email, full_name, password_hash, is_registered, version, created_at, created_by
)
VALUES
    ('c1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111', 
     '+380501112233', 'olena.koval@example.com', 'Олена Коваль', 
     '$2a$11$e8.Z/yY5tW9uGqL1vH5jEOzR9q4bVxY4oJ7d8v7f7x9m9q9w9e9r9', true, 1, 
     CURRENT_TIMESTAMP - INTERVAL '30 days', 'seed'),

    ('c1111111-1111-1111-1111-222222222222', '11111111-1111-1111-1111-111111111111', 
     '+380671112233', 'ivan.shevchenko@example.com', 'Іван Шевченко', 
     NULL, false, 1, 
     CURRENT_TIMESTAMP - INTERVAL '25 days', 'seed'),

    ('c2222222-2222-2222-2222-111111111111', '22222222-2222-2222-2222-222222222222', 
     '+380931112233', 'andriy.melnyk@example.com', 'Андрій Мельник', 
     '$2a$11$e8.Z/yY5tW9uGqL1vH5jEOzR9q4bVxY4oJ7d8v7f7x9m9q9w9e9r9', true, 1, 
     CURRENT_TIMESTAMP - INTERVAL '20 days', 'seed')
ON CONFLICT (id) DO NOTHING;


-- 2. ЛОКАЛЬНА РЕПЛІКА ТОВАРІВ (PRODUCTS_LOCAL)
INSERT INTO products_local (
    id, store_id, name, price, stock, is_active, source_updated_at, created_at, created_by
)
VALUES
    -- Товари Магазину 1 (Магія Парфумів)
    ('d1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111', 
     'Maison Francis Kurkdjian Baccarat Rouge 540 70ml', 8500.00, 15, true, 
     CURRENT_TIMESTAMP - INTERVAL '30 days', CURRENT_TIMESTAMP - INTERVAL '30 days', 'seed'),

    ('d1111111-1111-1111-1111-222222222222', '11111111-1111-1111-1111-111111111111', 
     'Dior Sauvage Eau de Parfum 100ml', 4200.00, 25, true, 
     CURRENT_TIMESTAMP - INTERVAL '30 days', CURRENT_TIMESTAMP - INTERVAL '30 days', 'seed'),

    ('d1111111-1111-1111-1111-333333333333', '11111111-1111-1111-1111-111111111111', 
     'Tom Ford Tobacco Vanille 50ml', 7300.00, 8, true, 
     CURRENT_TIMESTAMP - INTERVAL '30 days', CURRENT_TIMESTAMP - INTERVAL '30 days', 'seed'),

    ('d1111111-1111-1111-1111-444444444444', '11111111-1111-1111-1111-111111111111', 
     'Chanel Bleu de Chanel 100ml', 5100.00, 0, false, 
     CURRENT_TIMESTAMP - INTERVAL '30 days', CURRENT_TIMESTAMP - INTERVAL '30 days', 'seed'),

    -- Товари Магазину 2 (TechStore Pro)
    ('d2222222-2222-2222-2222-111111111111', '22222222-2222-2222-2222-222222222222', 
     'Навушники бездротові Sony WH-1000XM5 Black', 13999.00, 12, true, 
     CURRENT_TIMESTAMP - INTERVAL '25 days', CURRENT_TIMESTAMP - INTERVAL '25 days', 'seed'),

    ('d2222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', 
     'Механічна клавіатура Keychron K2 Pro Wireless', 4500.00, 18, true, 
     CURRENT_TIMESTAMP - INTERVAL '25 days', CURRENT_TIMESTAMP - INTERVAL '25 days', 'seed')
ON CONFLICT (id) DO NOTHING;


-- 3. ЗАМОВЛЕННЯ (ORDERS)
INSERT INTO orders (
    id, customer_id, store_id, status, payment_method, total_amount, version, created_at, created_by
)
VALUES
    ('019273c8-0001-7000-8000-000000000001', 'c1111111-1111-1111-1111-111111111111', 
     '11111111-1111-1111-1111-111111111111', 'Paid', 'Prepaid', 8500.00, 2, 
     CURRENT_TIMESTAMP - INTERVAL '10 days', 'seed'),

    ('019273c8-0002-7000-8000-000000000002', 'c1111111-1111-1111-1111-222222222222', 
     '11111111-1111-1111-1111-111111111111', 'Shipped', 'CashOnDelivery', 4200.00, 2, 
     CURRENT_TIMESTAMP - INTERVAL '5 days', 'seed'),

    ('019273c8-0003-7000-8000-000000000003', 'c2222222-2222-2222-2222-111111111111', 
     '22222222-2222-2222-2222-222222222222', 'Cancelled', 'Prepaid', 13999.00, 3, 
     CURRENT_TIMESTAMP - INTERVAL '3 days', 'seed'),

    ('019273c8-0004-7000-8000-000000000004', 'c1111111-1111-1111-1111-111111111111', 
     '11111111-1111-1111-1111-111111111111', 'Submitted', 'Prepaid', 7300.00, 1, 
     CURRENT_TIMESTAMP - INTERVAL '1 days', 'seed')
ON CONFLICT (id) DO NOTHING;


-- 4. ДЕТАЛІ ДОСТАВКИ (ORDER_DETAILS)
INSERT INTO order_details (
    order_id, delivery_provider, city, warehouse_address, recipient_phone, notes, created_at, created_by
)
VALUES
    ('019273c8-0001-7000-8000-000000000001', 'NovaPoshta', 'Чернівці', 'Відділення №1 (вул. Головна, 200)', 
     '+380501112233', 'Подзвонити перед доставкою', CURRENT_TIMESTAMP - INTERVAL '10 days', 'seed'),

    ('019273c8-0002-7000-8000-000000000002', 'NovaPoshta', 'Хотин', 'Відділення №1 (вул. Свято-Покровська, 50)', 
     '+380671112233', 'Оплата при отриманні', CURRENT_TIMESTAMP - INTERVAL '5 days', 'seed'),

    ('019273c8-0003-7000-8000-000000000003', 'UkrPoshta', 'Чернівці', 'Відділення 58000 (вул. Поштова, 1)', 
     '+380931112233', NULL, CURRENT_TIMESTAMP - INTERVAL '3 days', 'seed'),

    ('019273c8-0004-7000-8000-000000000004', 'NovaPoshta', 'Чернівці', 'Відділення №5', 
     '+380501112233', 'Тестове неоплачене', CURRENT_TIMESTAMP - INTERVAL '1 days', 'seed')
ON CONFLICT (order_id) DO NOTHING;


-- 5. ПОЗИЦІЇ ЗАМОВЛЕННЯ (ORDER_ITEMS)
INSERT INTO order_items (
    order_id, store_id, product_id, product_name, unit_price, quantity, created_at, created_by
)
VALUES
    ('019273c8-0001-7000-8000-000000000001', '11111111-1111-1111-1111-111111111111', 
     'd1111111-1111-1111-1111-111111111111', 'Maison Francis Kurkdjian Baccarat Rouge 540 70ml', 8500.00, 1, 
     CURRENT_TIMESTAMP - INTERVAL '10 days', 'seed'),

    ('019273c8-0002-7000-8000-000000000002', '11111111-1111-1111-1111-111111111111', 
     'd1111111-1111-1111-1111-222222222222', 'Dior Sauvage Eau de Parfum 100ml', 4200.00, 1, 
     CURRENT_TIMESTAMP - INTERVAL '5 days', 'seed'),

    ('019273c8-0003-7000-8000-000000000003', '22222222-2222-2222-2222-222222222222', 
     'd2222222-2222-2222-2222-111111111111', 'Навушники бездротові Sony WH-1000XM5 Black', 13999.00, 1, 
     CURRENT_TIMESTAMP - INTERVAL '3 days', 'seed'),

    ('019273c8-0004-7000-8000-000000000004', '11111111-1111-1111-1111-111111111111', 
     'd1111111-1111-1111-1111-333333333333', 'Tom Ford Tobacco Vanille 50ml', 7300.00, 1, 
     CURRENT_TIMESTAMP - INTERVAL '1 days', 'seed')
ON CONFLICT (order_id, product_id) DO NOTHING;


-- 6. ІСТОРІЯ СТАТУСІВ (ORDER_STATUS_HISTORY)
-- OVERRIDING SYSTEM VALUE стоїть після списку колонок!
INSERT INTO order_status_history (
    id, order_id, old_status, new_status, changed_by, changed_at, comment
)
OVERRIDING SYSTEM VALUE
VALUES
    (1, '019273c8-0001-7000-8000-000000000001', NULL, 'Submitted', 'system', CURRENT_TIMESTAMP - INTERVAL '10 days', 'Initial creation'),
    (2, '019273c8-0001-7000-8000-000000000001', 'Submitted', 'Paid', 'liqpay_webhook', CURRENT_TIMESTAMP - INTERVAL '9 days', 'Payment received'),
    (3, '019273c8-0002-7000-8000-000000000002', NULL, 'Submitted', 'system', CURRENT_TIMESTAMP - INTERVAL '5 days', 'Initial creation'),
    (4, '019273c8-0002-7000-8000-000000000002', 'Submitted', 'Shipped', 'manager', CURRENT_TIMESTAMP - INTERVAL '4 days', 'Dispatched with COD'),
    (5, '019273c8-0003-7000-8000-000000000003', NULL, 'Submitted', 'system', CURRENT_TIMESTAMP - INTERVAL '3 days', 'Initial creation'),
    (6, '019273c8-0003-7000-8000-000000000003', 'Submitted', 'Cancelled', 'customer', CURRENT_TIMESTAMP - INTERVAL '2 days', 'Customer changed mind'),
    (7, '019273c8-0004-7000-8000-000000000004', NULL, 'Submitted', 'system', CURRENT_TIMESTAMP - INTERVAL '1 days', 'Initial creation')
ON CONFLICT (id) DO NOTHING;


-- ВАЖЛИВО: Синхронізація лічильника (sequence) після ручної вставки id через OVERRIDING.
SELECT setval(
    pg_get_serial_sequence('order_status_history', 'id'),
    COALESCE((SELECT MAX(id) FROM order_status_history), 1)
);