-- Демонстраційні дані для catalog_db (Merchant Workspace & Catalog DB)

-- 1. КОРИСТУВАЧІ ПЛАТФОРМИ (USERS)
INSERT INTO users (id, email, password_hash, full_name, phone, system_role, version, created_at, created_by)
VALUES
    ('a1111111-1111-1111-1111-111111111111', 'owner.parfums@example.com', '$2a$11$e8.Z/yY5tW9uGqL1vH5jEOzR9q4bVxY4oJ7d8v7f7x9m9q9w9e9r9', 'Богдан Рибонька', '+380509998877', 'Merchant', 1, CURRENT_TIMESTAMP - INTERVAL '30 days', 'seed'),
    ('a2222222-2222-2222-2222-111111111111', 'owner.tech@example.com', '$2a$11$e8.Z/yY5tW9uGqL1vH5jEOzR9q4bVxY4oJ7d8v7f7x9m9q9w9e9r9', 'Олександр Петренко', '+380679998877', 'Merchant', 1, CURRENT_TIMESTAMP - INTERVAL '25 days', 'seed'),
    ('a1111111-1111-1111-1111-222222222222', 'manager.parfums@example.com', '$2a$11$e8.Z/yY5tW9uGqL1vH5jEOzR9q4bVxY4oJ7d8v7f7x9m9q9w9e9r9', 'Тетяна Дмитрук', '+380939998877', 'Staff', 1, CURRENT_TIMESTAMP - INTERVAL '20 days', 'seed')
ON CONFLICT (id) DO NOTHING;


-- 2. МАГАЗИНИ (STORES)
INSERT INTO stores (id, name, slug, owner_id, category, version, created_at, created_by)
VALUES
    ('11111111-1111-1111-1111-111111111111', 'Магія Парфумів', 'magia-parfumiv', 'a1111111-1111-1111-1111-111111111111', 'Парфумерія та косметика', 1, CURRENT_TIMESTAMP - INTERVAL '30 days', 'seed'),
    ('22222222-2222-2222-2222-222222222222', 'TechStore Pro', 'techstore-pro', 'a2222222-2222-2222-2222-111111111111', 'Комп’ютерна техніка та аудіо', 1, CURRENT_TIMESTAMP - INTERVAL '25 days', 'seed')
ON CONFLICT (id) DO NOTHING;


-- 3. КАТЕГОРІЇ (CATEGORIES)
INSERT INTO categories (id, store_id, parent_category_id, name, slug, version, created_at, created_by)
VALUES
    ('ca011111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111', NULL, 'Парфумерія', 'parfums', 1, CURRENT_TIMESTAMP - INTERVAL '30 days', 'seed'),
    ('ca011111-1111-1111-1111-222222222222', '11111111-1111-1111-1111-111111111111', 'ca011111-1111-1111-1111-111111111111', 'Нішеві парфуми', 'niche-parfums', 1, CURRENT_TIMESTAMP - INTERVAL '29 days', 'seed'),
    ('ca011111-1111-1111-1111-333333333333', '11111111-1111-1111-1111-111111111111', 'ca011111-1111-1111-1111-111111111111', 'Люксова парфумерія', 'luxury-parfums', 1, CURRENT_TIMESTAMP - INTERVAL '29 days', 'seed'),
    ('ca022222-2222-2222-2222-111111111111', '22222222-2222-2222-2222-222222222222', NULL, 'Аудіотехніка', 'audio', 1, CURRENT_TIMESTAMP - INTERVAL '25 days', 'seed'),
    ('ca022222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', NULL, 'Периферія', 'peripherals', 1, CURRENT_TIMESTAMP - INTERVAL '25 days', 'seed')
ON CONFLICT (id) DO NOTHING;


-- 4. ТОВАРИ (PRODUCTS)
INSERT INTO products (id, store_id, category_id, name, price, sku, is_active, version, created_at, created_by)
VALUES
    ('d1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111', 'ca011111-1111-1111-1111-222222222222', 'Maison Francis Kurkdjian Baccarat Rouge 540 70ml', 8500.00, 'MFK-BR540-70', true, 1, CURRENT_TIMESTAMP - INTERVAL '28 days', 'seed'),
    ('d1111111-1111-1111-1111-222222222222', '11111111-1111-1111-1111-111111111111', 'ca011111-1111-1111-1111-333333333333', 'Dior Sauvage Eau de Parfum 100ml', 4200.00, 'DIOR-SVG-100', true, 1, CURRENT_TIMESTAMP - INTERVAL '28 days', 'seed'),
    ('d1111111-1111-1111-1111-333333333333', '11111111-1111-1111-1111-111111111111', 'ca011111-1111-1111-1111-222222222222', 'Tom Ford Tobacco Vanille 50ml', 7300.00, 'TF-TV-50', true, 1, CURRENT_TIMESTAMP - INTERVAL '20 days', 'seed'),
    ('d1111111-1111-1111-1111-444444444444', '11111111-1111-1111-1111-111111111111', 'ca011111-1111-1111-1111-333333333333', 'Chanel Bleu de Chanel 100ml', 5100.00, 'CHNL-BDC-100', false, 1, CURRENT_TIMESTAMP - INTERVAL '15 days', 'seed'),
    ('d2222222-2222-2222-2222-111111111111', '22222222-2222-2222-2222-222222222222', 'ca022222-2222-2222-2222-111111111111', 'Навушники бездротові Sony WH-1000XM5 Black', 13999.00, 'SONY-WH1000XM5-BLK', true, 1, CURRENT_TIMESTAMP - INTERVAL '22 days', 'seed'),
    ('d2222222-2222-2222-2222-222222222222', '22222222-2222-2222-2222-222222222222', 'ca022222-2222-2222-2222-222222222222', 'Механічна клавіатура Keychron K2 Pro Wireless', 4500.00, 'KEYCHRON-K2PRO-RED', true, 1, CURRENT_TIMESTAMP - INTERVAL '20 days', 'seed')
ON CONFLICT (id) DO NOTHING;


-- 5. ДЕТАЛІ ТОВАРУ (PRODUCT_DETAILS)
INSERT INTO product_details (product_id, characteristics_json, created_at, created_by)
VALUES
    ('d1111111-1111-1111-1111-111111111111', '{"volume_ml": 70, "type": "Extrait de Parfum", "notes": ["жасмин", "шафран", "сіра амбра"], "country": "Франція"}'::jsonb, CURRENT_TIMESTAMP, 'seed'),
    ('d1111111-1111-1111-1111-222222222222', '{"volume_ml": 100, "type": "Eau de Parfum", "notes": ["бергамот", "сичуанський перець", "амброксан"], "country": "Франція"}'::jsonb, CURRENT_TIMESTAMP, 'seed'),
    ('d1111111-1111-1111-1111-333333333333', '{"volume_ml": 50, "type": "Eau de Parfum", "notes": ["тютюновий лист", "ваніль", "какао"], "country": "США"}'::jsonb, CURRENT_TIMESTAMP, 'seed'),
    ('d1111111-1111-1111-1111-444444444444', '{"volume_ml": 100, "type": "Eau de Parfum", "notes": ["грейпфрут", "кедр", "сандал"], "country": "Франція"}'::jsonb, CURRENT_TIMESTAMP, 'seed'),
    ('d2222222-2222-2222-2222-111111111111', '{"wireless": true, "anc": true, "battery_life_hours": 30, "weight_g": 250, "bluetooth": "5.2"}'::jsonb, CURRENT_TIMESTAMP, 'seed'),
    ('d2222222-2222-2222-2222-222222222222', '{"layout": "75%", "switches": "K Pro Red", "hotswap": true, "backlight": "RGB", "connectivity": ["BT 5.1", "Type-C"]}'::jsonb, CURRENT_TIMESTAMP, 'seed')
ON CONFLICT (product_id) DO NOTHING;


-- 6. МЕДІА ТОВАРІВ (PRODUCT_IMAGES)
INSERT INTO product_images (id, product_id, url, display_order, created_at, created_by)
VALUES
    ('fa011111-1111-1111-1111-111111111111', 'd1111111-1111-1111-1111-111111111111', 'https://cdn.example.com/products/baccarat-rouge-540-front.jpg', 0, CURRENT_TIMESTAMP - INTERVAL '28 days', 'seed'),
    ('fa011111-1111-1111-1111-222222222222', 'd1111111-1111-1111-1111-111111111111', 'https://cdn.example.com/products/baccarat-rouge-540-box.jpg', 1, CURRENT_TIMESTAMP - INTERVAL '28 days', 'seed'),
    ('fa011111-1111-1111-1111-333333333333', 'd1111111-1111-1111-1111-222222222222', 'https://cdn.example.com/products/dior-sauvage-edp.jpg', 0, CURRENT_TIMESTAMP - INTERVAL '28 days', 'seed'),
    ('fa022222-2222-2222-2222-111111111111', 'd2222222-2222-2222-2222-111111111111', 'https://cdn.example.com/products/sony-wh1000xm5-side.jpg', 0, CURRENT_TIMESTAMP - INTERVAL '22 days', 'seed'),
    ('fa022222-2222-2222-2222-222222222222', 'd2222222-2222-2222-2222-222222222222', 'https://cdn.example.com/products/keychron-k2pro-top.jpg', 0, CURRENT_TIMESTAMP - INTERVAL '20 days', 'seed')
ON CONFLICT (id) DO NOTHING;


-- 7. СПІВРОБІТНИКИ МАГАЗИНУ (STORE_EMPLOYEES)
INSERT INTO store_employees (store_id, user_id, role, assigned_at, is_active, created_at, created_by)
VALUES
    ('11111111-1111-1111-1111-111111111111', 'a1111111-1111-1111-1111-111111111111', 'Owner', CURRENT_TIMESTAMP - INTERVAL '30 days', true, CURRENT_TIMESTAMP - INTERVAL '30 days', 'seed'),
    ('11111111-1111-1111-1111-111111111111', 'a1111111-1111-1111-1111-222222222222', 'CatalogEditor', CURRENT_TIMESTAMP - INTERVAL '20 days', true, CURRENT_TIMESTAMP - INTERVAL '20 days', 'seed'),
    ('22222222-2222-2222-2222-222222222222', 'a2222222-2222-2222-2222-111111111111', 'Owner', CURRENT_TIMESTAMP - INTERVAL '25 days', true, CURRENT_TIMESTAMP - INTERVAL '25 days', 'seed')
ON CONFLICT (store_id, user_id) DO NOTHING;