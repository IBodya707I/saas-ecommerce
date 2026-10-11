-- База даних: catalog_db (Merchant Workspace & Catalog DB)
-- 7 сутностей: 1:1, 1:N, рекурсивний 1:N, M:N (store_employees)

-- 1. ТАБЛИЦЯ КОРИСТУВАЧІВ ПЛАТФОРМИ (USERS)
CREATE TABLE IF NOT EXISTS users (
    id            UUID PRIMARY KEY,
    email         VARCHAR(256) NOT NULL,
    password_hash VARCHAR(256) NOT NULL,
    full_name     VARCHAR(150) NOT NULL,
    phone         VARCHAR(30) NULL,
    system_role   VARCHAR(50) NOT NULL DEFAULT 'Merchant',
    version       INT NOT NULL DEFAULT 1, -- Оптимістичне блокування

    created_at    TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by    VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at    TIMESTAMPTZ NULL,
    updated_by    VARCHAR(100) NULL,
    is_deleted    BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT ck_users_phone CHECK (phone IS NULL OR phone ~ '^\+[1-9][0-9]{7,14}$'),
    CONSTRAINT ck_users_system_role CHECK (system_role IN ('PlatformAdmin', 'Merchant', 'Staff'))
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_users_email_active ON users (lower(email)) WHERE is_deleted = false;
CREATE UNIQUE INDEX IF NOT EXISTS uq_users_phone_active ON users (phone) WHERE is_deleted = false AND phone IS NOT NULL;


-- 2. ТАБЛИЦЯ МАГАЗИНІВ МЕРЧАНТІВ (STORES)
CREATE TABLE IF NOT EXISTS stores (
    id          UUID PRIMARY KEY,
    name        VARCHAR(150) NOT NULL,
    slug        VARCHAR(100) NOT NULL,
    owner_id    UUID NOT NULL,
    category    VARCHAR(100) NOT NULL,
    version     INT NOT NULL DEFAULT 1, -- Оптимістичне блокування

    created_at  TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by  VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at  TIMESTAMPTZ NULL,
    updated_by  VARCHAR(100) NULL,
    is_deleted  BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT fk_stores_owner FOREIGN KEY (owner_id) REFERENCES users(id) ON DELETE RESTRICT
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_stores_slug_active ON stores (lower(slug)) WHERE is_deleted = false;


-- 3. ТАБЛИЦЯ КАТЕГОРІЙ КАТАЛОГУ (CATEGORIES)
CREATE TABLE IF NOT EXISTS categories (
    id                 UUID PRIMARY KEY,
    store_id           UUID NOT NULL,
    parent_category_id UUID NULL,
    name               VARCHAR(100) NOT NULL,
    slug               VARCHAR(100) NOT NULL,
    version            INT NOT NULL DEFAULT 1,

    created_at         TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by         VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at         TIMESTAMPTZ NULL,
    updated_by         VARCHAR(100) NULL,
    is_deleted         BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT fk_categories_store FOREIGN KEY (store_id) REFERENCES stores(id) ON DELETE CASCADE,
    -- Унікальність для складеного ключа (забезпечує ізоляцію тенантів)
    CONSTRAINT uq_categories_store_id UNIQUE (store_id, id),
    -- Рекурсивний зв'язок 1:N із перевіркою приналежності до одного магазину
	CONSTRAINT fk_categories_parent FOREIGN KEY (store_id, parent_category_id) 
    	REFERENCES categories(store_id, id) ON DELETE SET NULL (parent_category_id),    
	CONSTRAINT ck_categories_not_self_parent CHECK (parent_category_id IS NULL OR parent_category_id <> id)
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_categories_store_slug_active ON categories (store_id, lower(slug)) WHERE is_deleted = false;


-- 4. ТАБЛИЦЯ ТОВАРІВ (PRODUCTS)
CREATE TABLE IF NOT EXISTS products (
    id                   UUID PRIMARY KEY,
    store_id             UUID NOT NULL,
    category_id          UUID NOT NULL,
    name                 VARCHAR(200) NOT NULL,
    price                NUMERIC(18,2) NOT NULL,
    sku                  VARCHAR(100) NOT NULL,
    is_active            BOOLEAN NOT NULL DEFAULT true,
    version              INT NOT NULL DEFAULT 1,

    created_at           TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by           VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at           TIMESTAMPTZ NULL,
    updated_by           VARCHAR(100) NULL,
    is_deleted           BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT fk_products_store FOREIGN KEY (store_id) REFERENCES stores(id) ON DELETE CASCADE,
    -- Жорстка ізоляція: товар може мати категорію ТІЛЬКИ свого магазину
    CONSTRAINT fk_products_category FOREIGN KEY (store_id, category_id) REFERENCES categories(store_id, id) ON DELETE RESTRICT,
    CONSTRAINT ck_products_price CHECK (price > 0)
);
CREATE UNIQUE INDEX IF NOT EXISTS uq_products_store_sku_active ON products (store_id, sku) WHERE is_deleted = false;


-- 5. ДЕТАЛІ ТОВАРУ (PRODUCT_DETAILS) - ЧИСТИЙ ЗВ'ЯЗОК 1:1 (Вимога AutoCheck)
-- Primary Key одночасно є Foreign Key
CREATE TABLE IF NOT EXISTS product_details (
    product_id           UUID PRIMARY KEY,
    characteristics_json JSONB NULL,
    description          TEXT NULL,

    created_at           TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by           VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at           TIMESTAMPTZ NULL,
    updated_by           VARCHAR(100) NULL,
    is_deleted           BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT fk_product_details_products FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);


-- 6. ТАБЛИЦЯ МЕДІА ТОВАРІВ (PRODUCT_IMAGES) - Зв'язок 1:N
CREATE TABLE IF NOT EXISTS product_images (
    id            UUID PRIMARY KEY,
    product_id    UUID NOT NULL,
    url           VARCHAR(500) NOT NULL,
    display_order INT NOT NULL DEFAULT 0,

    created_at    TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by    VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at    TIMESTAMPTZ NULL,
    updated_by    VARCHAR(100) NULL,
    is_deleted    BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT fk_product_images_product FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
    CONSTRAINT ck_product_images_order CHECK (display_order >= 0)
);


-- 7. ТАБЛИЦЯ ПРАЦІВНИКІВ МАГАЗИНУ (STORE_EMPLOYEES) - M:N МІСТ
CREATE TABLE IF NOT EXISTS store_employees (
    store_id    UUID NOT NULL,
    user_id     UUID NOT NULL,
    role        VARCHAR(50) NOT NULL DEFAULT 'Manager',
    assigned_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_active   BOOLEAN NOT NULL DEFAULT true,

    created_at  TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by  VARCHAR(100) NOT NULL DEFAULT 'system',
    updated_at  TIMESTAMPTZ NULL,
    updated_by  VARCHAR(100) NULL,
    is_deleted  BOOLEAN NOT NULL DEFAULT false,

    CONSTRAINT pk_store_employees PRIMARY KEY (store_id, user_id),
    CONSTRAINT fk_store_employees_store FOREIGN KEY (store_id) REFERENCES stores(id) ON DELETE CASCADE,
    CONSTRAINT fk_store_employees_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT ck_store_employees_role CHECK (role IN ('Owner', 'Manager', 'CatalogEditor', 'OrderOperator'))
);


-- Запит 1: Отримання каталогу активних товарів магазину з фільтром за категорією (Index-Only Scan)
-- Запит: SELECT id, name, price, sku FROM products WHERE store_id = $1 AND category_id = $2 AND is_active = true AND is_deleted = false;
CREATE INDEX IF NOT EXISTS ix_products_store_category ON products (store_id, category_id) INCLUDE (id, name, price, sku) WHERE is_active = true AND is_deleted = false;

-- Запит 2: Вибірка підкатегорій для побудови дерева навігації
-- Запит: SELECT id, name, slug FROM categories WHERE parent_category_id = $1 AND is_deleted = false;
CREATE INDEX IF NOT EXISTS ix_categories_parent ON categories (parent_category_id) WHERE is_deleted = false;

-- Запит 3: Отримання списку зображень для картки товару у порядку відображення
-- Запит: SELECT url, display_order FROM product_images WHERE product_id = $1 AND is_deleted = false ORDER BY display_order ASC;
CREATE INDEX IF NOT EXISTS ix_product_images_product_order ON product_images (product_id, display_order ASC) WHERE is_deleted = false;

-- Запит 4: Визначення магазинів, до яких користувач має доступ працівника
-- Запит: SELECT store_id, role FROM store_employees WHERE user_id = $1 AND is_active = true AND is_deleted = false;
CREATE INDEX IF NOT EXISTS ix_store_employees_user_active ON store_employees (user_id) WHERE is_active = true AND is_deleted = false;

-- Запит 5: Пошук магазинів конкретного власника
-- Запит: SELECT id, name, slug FROM stores WHERE owner_id = $1 AND is_deleted = false;
CREATE INDEX IF NOT EXISTS ix_stores_owner ON stores (owner_id) WHERE is_deleted = false;

-- Запит 6: Швидкий повнотекстовий пошук товарів за назвою (GIN)
-- Запит: SELECT id, name FROM products WHERE to_tsvector('simple', name) @@ to_tsquery('simple', $1) AND is_deleted = false;
CREATE INDEX IF NOT EXISTS ix_products_name_fts ON products USING gin (to_tsvector('simple', name)) WHERE is_deleted = false;

-- Запит 7: Пошук товарів за специфічними JSON-фільтрами (наприклад, об'єм 100мл) (GIN)
-- Запит: SELECT product_id FROM product_details WHERE characteristics_json @> '{"volume_ml": 100}'::jsonb AND is_deleted = false;
CREATE INDEX IF NOT EXISTS ix_product_details_characteristics_gin ON product_details USING gin (characteristics_json) WHERE is_deleted = false;