CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE TABLE users (
        id uuid NOT NULL,
        email character varying(256) NOT NULL,
        password_hash character varying(256) NOT NULL,
        full_name character varying(150) NOT NULL,
        phone character varying(30),
        system_role character varying(50) NOT NULL DEFAULT 'Merchant',
        version integer NOT NULL DEFAULT 1,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        created_by character varying(100) NOT NULL DEFAULT 'system',
        updated_at timestamp with time zone,
        updated_by character varying(100),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_users" PRIMARY KEY (id),
        CONSTRAINT ck_users_phone CHECK (phone IS NULL OR phone ~ '^\+[1-9][0-9]{7,14}$'),
        CONSTRAINT ck_users_system_role CHECK (system_role IN ('PlatformAdmin', 'Merchant', 'Staff'))
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE TABLE stores (
        id uuid NOT NULL,
        name character varying(150) NOT NULL,
        slug character varying(100) NOT NULL,
        owner_id uuid NOT NULL,
        category character varying(100) NOT NULL,
        version integer NOT NULL DEFAULT 1,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        created_by character varying(100) NOT NULL DEFAULT 'system',
        updated_at timestamp with time zone,
        updated_by character varying(100),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_stores" PRIMARY KEY (id),
        CONSTRAINT "FK_stores_users_owner_id" FOREIGN KEY (owner_id) REFERENCES users (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE TABLE categories (
        id uuid NOT NULL,
        store_id uuid NOT NULL,
        parent_category_id uuid,
        name character varying(100) NOT NULL,
        slug character varying(100) NOT NULL,
        version integer NOT NULL DEFAULT 1,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        created_by character varying(100) NOT NULL DEFAULT 'system',
        updated_at timestamp with time zone,
        updated_by character varying(100),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_categories" PRIMARY KEY (id),
        CONSTRAINT ck_categories_not_self_parent CHECK (parent_category_id IS NULL OR parent_category_id <> id),
        CONSTRAINT "FK_categories_categories_parent_category_id" FOREIGN KEY (parent_category_id) REFERENCES categories (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_categories_stores_store_id" FOREIGN KEY (store_id) REFERENCES stores (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE TABLE store_employees (
        store_id uuid NOT NULL,
        user_id uuid NOT NULL,
        role character varying(50) NOT NULL DEFAULT 'Manager',
        assigned_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        is_active boolean NOT NULL DEFAULT TRUE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        created_by character varying(100) NOT NULL DEFAULT 'system',
        updated_at timestamp with time zone,
        updated_by character varying(100),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_store_employees" PRIMARY KEY (store_id, user_id),
        CONSTRAINT ck_store_employees_role CHECK (role IN ('Owner', 'Manager', 'CatalogEditor', 'OrderOperator')),
        CONSTRAINT "FK_store_employees_stores_store_id" FOREIGN KEY (store_id) REFERENCES stores (id) ON DELETE CASCADE,
        CONSTRAINT "FK_store_employees_users_user_id" FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE TABLE products (
        id uuid NOT NULL,
        store_id uuid NOT NULL,
        category_id uuid NOT NULL,
        name character varying(200) NOT NULL,
        price numeric(18,2) NOT NULL,
        sku character varying(100) NOT NULL,
        available_stock integer NOT NULL DEFAULT 0,
        is_active boolean NOT NULL DEFAULT TRUE,
        version integer NOT NULL DEFAULT 1,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        created_by character varying(100) NOT NULL DEFAULT 'system',
        updated_at timestamp with time zone,
        updated_by character varying(100),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_products" PRIMARY KEY (id),
        CONSTRAINT ck_products_price CHECK (price > 0),
        CONSTRAINT "FK_products_categories_category_id" FOREIGN KEY (category_id) REFERENCES categories (id) ON DELETE RESTRICT,
        CONSTRAINT "FK_products_stores_store_id" FOREIGN KEY (store_id) REFERENCES stores (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE TABLE product_details (
        product_id uuid NOT NULL,
        characteristics_json jsonb NOT NULL DEFAULT '{}',
        description text,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        created_by character varying(100) NOT NULL DEFAULT 'system',
        updated_at timestamp with time zone,
        updated_by character varying(100),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_product_details" PRIMARY KEY (product_id),
        CONSTRAINT "FK_product_details_products_product_id" FOREIGN KEY (product_id) REFERENCES products (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE TABLE product_images (
        id uuid NOT NULL,
        product_id uuid NOT NULL,
        url character varying(500) NOT NULL,
        display_order integer NOT NULL DEFAULT 0,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        created_by character varying(100) NOT NULL DEFAULT 'system',
        updated_at timestamp with time zone,
        updated_by character varying(100),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        CONSTRAINT "PK_product_images" PRIMARY KEY (id),
        CONSTRAINT ck_product_images_order CHECK (display_order >= 0),
        CONSTRAINT "FK_product_images_products_product_id" FOREIGN KEY (product_id) REFERENCES products (id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    INSERT INTO users (id, created_at, created_by, email, full_name, password_hash, phone, system_role, updated_at, updated_by, version)
    VALUES ('a1111111-1111-1111-1111-111111111111', TIMESTAMPTZ '2026-01-01T00:00:00Z', 'seed', 'owner.parfums@example.com', 'Богдан Рибонька', '$2a$11$e8.Z/yY5tW9uGqL1vH5jEOzR9q4bVxY4oJ7d8v7f7x9m9q9w9e9r9', '+380509998877', 'Merchant', NULL, NULL, 1);
    INSERT INTO users (id, created_at, created_by, email, full_name, password_hash, phone, system_role, updated_at, updated_by, version)
    VALUES ('a1111111-1111-1111-1111-222222222222', TIMESTAMPTZ '2026-01-01T00:00:00Z', 'seed', 'manager.parfums@example.com', 'Тетяна Дмитрук', '$2a$11$e8.Z/yY5tW9uGqL1vH5jEOzR9q4bVxY4oJ7d8v7f7x9m9q9w9e9r9', '+380939998877', 'Staff', NULL, NULL, 1);
    INSERT INTO users (id, created_at, created_by, email, full_name, password_hash, phone, system_role, updated_at, updated_by, version)
    VALUES ('a2222222-2222-2222-2222-111111111111', TIMESTAMPTZ '2026-01-01T00:00:00Z', 'seed', 'owner.tech@example.com', 'Олександр Петренко', '$2a$11$e8.Z/yY5tW9uGqL1vH5jEOzR9q4bVxY4oJ7d8v7f7x9m9q9w9e9r9', '+380679998877', 'Merchant', NULL, NULL, 1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    INSERT INTO stores (id, category, created_at, created_by, name, owner_id, slug, updated_at, updated_by, version)
    VALUES ('11111111-1111-1111-1111-111111111111', 'Парфумерія та косметика', TIMESTAMPTZ '2026-01-01T00:00:00Z', 'seed', 'Магія Парфумів', 'a1111111-1111-1111-1111-111111111111', 'magia-parfumiv', NULL, NULL, 1);
    INSERT INTO stores (id, category, created_at, created_by, name, owner_id, slug, updated_at, updated_by, version)
    VALUES ('22222222-2222-2222-2222-222222222222', 'Комп’ютерна техніка та аудіо', TIMESTAMPTZ '2026-01-01T00:00:00Z', 'seed', 'TechStore Pro', 'a2222222-2222-2222-2222-111111111111', 'techstore-pro', NULL, NULL, 1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    INSERT INTO categories (id, created_at, created_by, name, parent_category_id, slug, store_id, updated_at, updated_by, version)
    VALUES ('ca011111-1111-1111-1111-111111111111', TIMESTAMPTZ '2026-01-01T00:00:00Z', 'seed', 'Парфумерія', NULL, 'parfums', '11111111-1111-1111-1111-111111111111', NULL, NULL, 1);
    INSERT INTO categories (id, created_at, created_by, name, parent_category_id, slug, store_id, updated_at, updated_by, version)
    VALUES ('ca022222-2222-2222-2222-111111111111', TIMESTAMPTZ '2026-01-01T00:00:00Z', 'seed', 'Аудіотехніка', NULL, 'audio', '22222222-2222-2222-2222-222222222222', NULL, NULL, 1);
    INSERT INTO categories (id, created_at, created_by, name, parent_category_id, slug, store_id, updated_at, updated_by, version)
    VALUES ('ca022222-2222-2222-2222-222222222222', TIMESTAMPTZ '2026-01-01T00:00:00Z', 'seed', 'Периферія', NULL, 'peripherals', '22222222-2222-2222-2222-222222222222', NULL, NULL, 1);
    INSERT INTO categories (id, created_at, created_by, name, parent_category_id, slug, store_id, updated_at, updated_by, version)
    VALUES ('ca011111-1111-1111-1111-222222222222', TIMESTAMPTZ '2026-01-01T00:00:00Z', 'seed', 'Нішеві парфуми', 'ca011111-1111-1111-1111-111111111111', 'niche-parfums', '11111111-1111-1111-1111-111111111111', NULL, NULL, 1);
    INSERT INTO categories (id, created_at, created_by, name, parent_category_id, slug, store_id, updated_at, updated_by, version)
    VALUES ('ca011111-1111-1111-1111-333333333333', TIMESTAMPTZ '2026-01-01T00:00:00Z', 'seed', 'Люксова парфумерія', 'ca011111-1111-1111-1111-111111111111', 'luxury-parfums', '11111111-1111-1111-1111-111111111111', NULL, NULL, 1);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE INDEX "IX_categories_parent_category_id" ON categories (parent_category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE UNIQUE INDEX uq_categories_store_slug_active ON categories (store_id, slug) WHERE is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE INDEX ix_product_details_characteristics_gin ON product_details USING gin (characteristics_json);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE INDEX ix_product_images_product_order ON product_images (product_id, display_order) WHERE is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE INDEX "IX_products_category_id" ON products (category_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE UNIQUE INDEX uq_products_store_sku_active ON products (store_id, sku) WHERE is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE INDEX ix_store_employees_user_active ON store_employees (user_id) WHERE is_active = true AND is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE INDEX "IX_stores_owner_id" ON stores (owner_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE UNIQUE INDEX uq_stores_slug_active ON stores (slug) WHERE is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    CREATE UNIQUE INDEX uq_users_email_active ON users (email) WHERE is_deleted = false;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261025140040_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261025140040_InitialCreate', '10.0.4');
    END IF;
END $EF$;
COMMIT;

