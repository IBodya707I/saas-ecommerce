namespace Reviews.Infrastructure.Seeding;

using MongoDB.Driver;
using Reviews.Domain.Entities;
using Reviews.Domain.ValueObjects;
using Reviews.Infrastructure.Persistence;

public class ReviewSeeder : IDataSeeder
{
    private readonly MongoDbContext _context;

    public ReviewSeeder(MongoDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        var reviewsCount = await _context.Reviews.EstimatedDocumentCountAsync(cancellationToken: ct);
        if (reviewsCount == 0)
        {
            // Відгук 1 (з db/p3/seed.js: Олена Коваль, парфуми з відповіддю магазину)
            var review1 = new Review(
                1,
                "Maison Francis Kurkdjian Baccarat Rouge 540 70ml",
                new Author("c1111111-1111-1111-1111-111111111111", "Олена Коваль"),
                5,
                "Неймовірний шлейф та стійкість! Оригінал 100%, перевірила за батч-кодом. Доставили наступного дня.");
            review1.AddComment("Магія Парфумів (Менеджер)", "Олено, дякуємо за відгук!");

            var review2 = new Review(
                2,
                "Навушники бездротові Sony WH-1000XM5 Black",
                new Author("c2222222-2222-2222-2222-111111111111", "Андрій Мельник"),
                4,
                "Шумодав топовий, але кейс дещо більший за попередню модель XM4.");

            var review3 = new Review(
                3,
                "Dior Sauvage Eau de Parfum 100ml",
                new Author("c3333333-3333-3333-3333-111111111111", "Іван Шевченко"),
                5,
                "Швидко відправили накладеним платежем Новою поштою. Гарне пакування посилки.");

            var review4 = new Review(
                42,
                "Ноутбук X15",
                new Author("u-21", "Олена"),
                5,
                "Чудовий екран та потужний процесор.");

            await _context.Reviews.InsertManyAsync(new[] { review1, review2, review3, review4 }, cancellationToken: ct);
        }

        var showcasesCount = await _context.StorefrontShowcases.EstimatedDocumentCountAsync(cancellationToken: ct);
        if (showcasesCount == 0)
        {
            var showcase1 = new StorefrontShowcase(
                "11111111-1111-1111-1111-111111111111",
                "magia-parfumiv",
                "Магія Парфумів",
                new Branding("#800020", "Playfair Display, serif", "https://cdn.example.com/stores/magia-logo.png"),
                "Пн-Пт: 09:00 - 20:00, Сб-Нд: 10:00 - 18:00");

            var showcase2 = new StorefrontShowcase(
                "22222222-2222-2222-2222-222222222222",
                "techstore-pro",
                "TechStore Pro",
                new Branding("#0A84FF", "Inter, sans-serif"),
                "Щодня: 10:00 - 21:00");

            await _context.StorefrontShowcases.InsertManyAsync(new[] { showcase1, showcase2 }, cancellationToken: ct);
        }

        var discussionsCount = await _context.ProductDiscussions.EstimatedDocumentCountAsync(cancellationToken: ct);
        if (discussionsCount == 0)
        {
            var discussion = new ProductDiscussion(3, "Питання щодо стійкості та партії");
            discussion.IncrementMessageCount();
            discussion.IncrementMessageCount();

            await _context.ProductDiscussions.InsertOneAsync(discussion, cancellationToken: ct);
        }
    }
}
