// База даних: showcase_db

const dbName = "showcase_db";
const dbInstance = db.getSiblingDB(dbName);

dbInstance.storefront_showcases.drop();
dbInstance.reviews.drop();
dbInstance.product_discussions.drop();
dbInstance.discussion_messages.drop();

// 1. КОЛЕКЦІЯ: storefront_showcases (Патерн: Вбудовування / Embedded)
dbInstance.createCollection("storefront_showcases", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["storeId", "slug", "storeName", "branding"],
            properties: {
                storeId: {
                    bsonType: "string",
                    pattern: "^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$"
                },
                slug: { bsonType: "string" },
                storeName: { bsonType: "string" },
                branding: {
                    bsonType: "object",
                    required: ["color", "fontFamily"],
                    properties: {
                        color: { bsonType: "string" },
                        fontFamily: { bsonType: "string" }
                        // logo може бути відсутнім
                    }
                }
                // banners, workSchedule, socialLinks, trustBadges - не обов'язкові, можуть бути відсутніми
            }
        }
    }
});

// Запит: db.storefront_showcases.findOne({ storeId: "..." });
dbInstance.storefront_showcases.createIndex({ storeId: 1 }, { unique: true });
// Запит: db.storefront_showcases.findOne({ slug: "..." });
dbInstance.storefront_showcases.createIndex({ slug: 1 }, { unique: true });


// 2. КОЛЕКЦІЯ: reviews (Справжній поліморфізм: різні набори ключів)
dbInstance.createCollection("reviews", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["targetType", "storeId", "rating", "author", "status", "createdAt"],
            properties: {
                targetType: { enum: ["product", "store"] },
                storeId: { bsonType: "string" },
                rating: { bsonType: "int", minimum: 1, maximum: 5 },
                author: {
                    bsonType: "object",
                    required: ["customerId", "name", "verifiedBuyer"],
                    properties: {
                        customerId: { bsonType: "string" },
                        name: { bsonType: "string" },
                        verifiedBuyer: { bsonType: "bool" }
                    }
                },
                status: { enum: ["Published", "PendingModeration", "Rejected"] },
                createdAt: { bsonType: "date" },
                productId: { bsonType: "string" },
                orderId: { bsonType: "string" },
                photos: { bsonType: "array", items: { bsonType: "string" } },
                productName: { bsonType: "string" },
                text: { bsonType: "string" },
                deliveryRating: { bsonType: "int", minimum: 1, maximum: 5 },
                comments: { bsonType: "array" }
            }
        }
    }
});

// Запит: db.reviews.find({ storeId: "...", productId: "...", status: "Published" }).sort({ createdAt: -1 });
dbInstance.reviews.createIndex({ storeId: 1, productId: 1, status: 1, createdAt: -1 });
// Запит: db.reviews.find({ "author.customerId": "..." });
dbInstance.reviews.createIndex({ "author.customerId": 1 });


// 3. КОЛЕКЦІЯ: product_discussions (Subset Pattern)
dbInstance.createCollection("product_discussions", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["storeId", "productId", "title", "status", "createdAt", "messageCount"],
            properties: {
                storeId: { bsonType: "string" },
                productId: { bsonType: "string" },
                title: { bsonType: "string" },
                status: { enum: ["Open", "Closed", "Archived"] },
                createdAt: { bsonType: "date" },
                productName: { bsonType: "string" },
                messageCount: { bsonType: "int", minimum: 0 }
            }
        }
    }
});

// Запит: db.product_discussions.find({ storeId: "...", productId: "...", status: "Open" }).sort({ createdAt: -1 });
dbInstance.product_discussions.createIndex({ storeId: 1, productId: 1, createdAt: -1 });


// 4. КОЛЕКЦІЯ: discussion_messages (1:N Referencing)
dbInstance.createCollection("discussion_messages", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["discussionId", "sender", "text", "sentAt", "storeId"],
            properties: {
                discussionId: { bsonType: "objectId" },
                sender: {
                    bsonType: "object",
                    required: ["type", "id", "name"],
                    properties: {
                        type: { enum: ["Customer", "Merchant", "Support"] },
                        id: { bsonType: "string" },
                        name: { bsonType: "string" }
                    }
                },
                text: { bsonType: "string" },
                sentAt: { bsonType: "date" },
                storeId: { bsonType: "string" }
            }
        }
    }
});

// Запит: db.discussion_messages.find({ discussionId: ObjectId("...") }).sort({ sentAt: 1 });
dbInstance.discussion_messages.createIndex({ discussionId: 1, sentAt: 1 });