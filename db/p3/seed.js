// Демонстраційні дані для showcase_db
// Демонструє справжній поліморфізм полів (відсутність зайвих null/[])


const dbName = "showcase_db";
const dbInstance = db.getSiblingDB(dbName);

// 1. ВІТРИНИ МАГАЗИНІВ (Різні структури контенту для різних бізнесів)
dbInstance.storefront_showcases.bulkWrite([
    {
        updateOne: {
            filter: { storeId: "11111111-1111-1111-1111-111111111111" },
            update: {
                $set: {
                    slug: "magia-parfumiv",
                    storeName: "Магія Парфумів",
                    branding: { logo: "https://cdn.example.com/stores/magia-logo.png", color: "#800020", fontFamily: "Playfair Display, serif" },
                    banners: [{ url: "https://cdn.example.com/banners/autumn-sale.jpg", altText: "Осінній розпродаж ніші" }],
                    workSchedule: "Пн-Пт: 09:00 - 20:00, Сб-Нд: 10:00 - 18:00",
                    socialLinks: { instagram: "https://instagram.com/magia_parfumiv", tiktok: "https://tiktok.com/@magia_parfumiv" }
                }
            },
            upsert: true
        }
    },
    {
        updateOne: {
            filter: { storeId: "22222222-2222-2222-2222-222222222222" },
            update: {
                $set: {
                    slug: "techstore-pro",
                    storeName: "TechStore Pro",
                    branding: { color: "#0A84FF", fontFamily: "Inter, sans-serif" }, // Тут немає поля logo
                    banners: [{ url: "https://cdn.example.com/banners/audio-fest.jpg", altText: "Фестиваль якісного звуку" }],
                    workSchedule: "Щодня: 10:00 - 21:00",
                    trustBadges: [ // Тут є бейджі довіри, яких немає в магазині 1
                        { icon: "shield-check", title: "Офіційна гарантія 24 місяці" },
                        { icon: "truck-fast", title: "Відправка в день замовлення" }
                    ]
                }
            },
            upsert: true
        }
    }
]);


// 2. ВІДГУКИ: Демонстрація гнучкості схеми 
// Документи з різними наборами полів!
dbInstance.reviews.bulkWrite([
    // Документ 1: Товарний відгук із денормалізованою назвою, фото та коментарем адміністрації
    {
        updateOne: {
            filter: { _id: ObjectId("66f0c2000000000000000001") },
            update: {
                $set: {
                    targetType: "product",
                    storeId: "11111111-1111-1111-1111-111111111111",
                    productId: "d1111111-1111-1111-1111-111111111111",
                    orderId: "019273c8-0001-7000-8000-000000000001",
                    rating: NumberInt(5),
                    author: { customerId: "c1111111-1111-1111-1111-111111111111", name: "Олена Коваль", verifiedBuyer: true },
                    photos: ["https://cdn.example.com/reviews/baccarat-unbox.jpg"],
                    productName: "Maison Francis Kurkdjian Baccarat Rouge 540 70ml",
                    text: "Неймовірний шлейф та стійкість! Оригінал 100%, перевірила за батч-кодом. Доставили наступного дня.",
                    comments: [{ author: "Магія Парфумів (Менеджер)", text: "Олено, дякуємо за відгук!", createdAt: new Date(Date.now() - 2 * 86400000) }],
                    status: "Published",
                    createdAt: new Date(Date.now() - 3 * 86400000)
                    // deliveryRating відсутнє
                }
            },
            upsert: true
        }
    },
    // Документ 2: Сервісний відгук на магазин
    {
        updateOne: {
            filter: { _id: ObjectId("66f0c2000000000000000002") },
            update: {
                $set: {
                    targetType: "store",
                    storeId: "11111111-1111-1111-1111-111111111111",
                    orderId: "019273c8-0002-7000-8000-000000000002",
                    rating: NumberInt(5),
                    author: { customerId: "c1111111-1111-1111-1111-222222222222", name: "Іван Шевченко", verifiedBuyer: true },
                    text: "Швидко відправили накладеним платежем Новою поштою. Гарне пакування посилки.",
                    status: "Published",
                    createdAt: new Date(Date.now() - 1 * 86400000),
                    deliveryRating: NumberInt(5)
                    // productId, photos, productName, comments повністю відсутні!
                }
            },
            upsert: true
        }
    },
    // Документ 3: Звичайний відгук без замовлення
    {
        updateOne: {
            filter: { _id: ObjectId("66f0c2000000000000000003") },
            update: {
                $set: {
                    targetType: "product",
                    storeId: "22222222-2222-2222-2222-222222222222",
                    productId: "d2222222-2222-2222-2222-111111111111",
                    rating: NumberInt(4),
                    author: { customerId: "c2222222-2222-2222-2222-111111111111", name: "Андрій Мельник", verifiedBuyer: false },
                    productName: "Навушники бездротові Sony WH-1000XM5 Black",
                    text: "Шумодав топовий, але кейс дещо більший за попередню модель XM4.",
                    status: "Published",
                    createdAt: new Date(Date.now() - 5 * 86400000)
                    // orderId, photos, comments, deliveryRating повністю відсутні!
                }
            },
            upsert: true
        }
    }
]);


// 3. ОБГОВОРЕННЯ ТА ПОВІДОМЛЕННЯ (1:N Referencing через discussionId)
const discId = ObjectId("66f0c3000000000000000001");

dbInstance.product_discussions.updateOne(
    { _id: discId },
    {
        $set: {
            storeId: "11111111-1111-1111-1111-111111111111",
            productId: "d1111111-1111-1111-1111-222222222222",
            title: "Питання щодо стійкості та партії",
            status: "Open",
            createdAt: new Date(Date.now() - 4 * 86400000),
            productName: "Dior Sauvage Eau de Parfum 100ml",
            messageCount: NumberInt(2),
            lastMessage: { text: "Доброго дня! Це партія 2024 року, стійкість до 10-12 годин.", sentAt: new Date(Date.now() - 3 * 86400000) }
        }
    },
    { upsert: true }
);

dbInstance.discussion_messages.bulkWrite([
    {
        updateOne: {
            filter: { _id: ObjectId("66f0c4000000000000000001") },
            update: {
                $set: {
                    discussionId: discId,
                    sender: { type: "Customer", id: "c1111111-1111-1111-1111-222222222222", name: "Іван Шевченко" },
                    text: "Підкажіть, будь ласка, який рік випуску поточної партії?",
                    sentAt: new Date(Date.now() - 4 * 86400000),
                    storeId: "11111111-1111-1111-1111-111111111111"
                }
            },
            upsert: true
        }
    },
    {
        updateOne: {
            filter: { _id: ObjectId("66f0c4000000000000000002") },
            update: {
                $set: {
                    discussionId: discId,
                    sender: { type: "Merchant", id: "a1111111-1111-1111-1111-111111111111", name: "Магія Парфумів" },
                    text: "Доброго дня! Це партія 2024 року, стійкість до 10-12 годин.",
                    sentAt: new Date(Date.now() - 3 * 86400000),
                    storeId: "11111111-1111-1111-1111-111111111111"
                }
            },
            upsert: true
        }
    }
]);