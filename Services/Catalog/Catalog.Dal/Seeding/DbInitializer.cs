using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Dal.Seeding;

public static class DbInitializer
{
    public static async Task SeedAsync(CatalogDbContext context, CancellationToken ct = default)
    {
        // Idempotent seeding (Criterion 9): check if records already exist
        if (await context.Products.AnyAsync(ct))
        {
            return;
        }

        var store1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var store2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var catNicheId = Guid.Parse("ca011111-1111-1111-1111-222222222222");
        var catLuxuryId = Guid.Parse("ca011111-1111-1111-1111-333333333333");
        var catAudioId = Guid.Parse("ca022222-2222-2222-2222-111111111111");
        var catPeripheralsId = Guid.Parse("ca022222-2222-2222-2222-222222222222");

        var userOwner1 = Guid.Parse("a1111111-1111-1111-1111-111111111111");
        var userOwner2 = Guid.Parse("a2222222-2222-2222-2222-111111111111");
        var userManager1 = Guid.Parse("a1111111-1111-1111-1111-222222222222");

        // 1. Store Employees (M:N with bridge fields)
        if (!await context.StoreEmployees.AnyAsync(ct))
        {
            var employees = new List<StoreEmployee>
            {
                new(store1Id, userOwner1, "Owner", DateTime.UtcNow.AddDays(-30), "seed"),
                new(store1Id, userManager1, "CatalogEditor", DateTime.UtcNow.AddDays(-20), "seed"),
                new(store2Id, userOwner2, "Owner", DateTime.UtcNow.AddDays(-25), "seed")
            };
            await context.StoreEmployees.AddRangeAsync(employees, ct);
        }

        // 2. Products with 1:1 Detail (jsonb Characteristics) and 1:N Images
        var p1 = new Product(
            Guid.Parse("d1111111-1111-1111-1111-111111111111"),
            store1Id,
            catNicheId,
            "Maison Francis Kurkdjian Baccarat Rouge 540 70ml",
            "MFK-BR540-70",
            8500.00m,
            25,
            "seed"
        );
        p1.SetDetail(new ProductDetail(
            p1.Id,
            "{\"brand\": \"Maison Francis Kurkdjian\", \"volume_ml\": \"70\", \"type\": \"Extrait de Parfum\", \"country\": \"Франція\"}",
            "Нішевий парфум найвищої концентрації з виразним шлейфом сірої амбри та жасмину.",
            "seed"
        ));

        var p2 = new Product(
            Guid.Parse("d1111111-1111-1111-1111-222222222222"),
            store1Id,
            catLuxuryId,
            "Dior Sauvage Eau de Parfum 100ml",
            "DIOR-SVG-100",
            4200.00m,
            40,
            "seed"
        );
        p2.SetDetail(new ProductDetail(
            p2.Id,
            "{\"brand\": \"Dior\", \"volume_ml\": \"100\", \"type\": \"Eau de Parfum\", \"country\": \"Франція\"}",
            "Легендарний свіжий і пряний аромат для чоловіків.",
            "seed"
        ));

        var p3 = new Product(
            Guid.Parse("d1111111-1111-1111-1111-333333333333"),
            store1Id,
            catNicheId,
            "Tom Ford Tobacco Vanille 50ml",
            "TF-TV-50",
            7300.00m,
            15,
            "seed"
        );
        p3.SetDetail(new ProductDetail(
            p3.Id,
            "{\"brand\": \"Tom Ford\", \"volume_ml\": \"50\", \"type\": \"Eau de Parfum\", \"country\": \"США\"}",
            "Теплий та пряний аромат східних спецій, тютюну та солодкої ванілі.",
            "seed"
        ));

        var p4 = new Product(
            Guid.Parse("d1111111-1111-1111-1111-444444444444"),
            store1Id,
            catLuxuryId,
            "Chanel Bleu de Chanel 100ml",
            "CHNL-BDC-100",
            5100.00m,
            10,
            "seed"
        );
        p4.SetDetail(new ProductDetail(
            p4.Id,
            "{\"brand\": \"Chanel\", \"volume_ml\": \"100\", \"type\": \"Eau de Parfum\", \"country\": \"Франція\"}",
            "Деревний аромат з нотами кедра та сандала.",
            "seed"
        ));
        p4.Deactivate("seed");

        var p5 = new Product(
            Guid.Parse("d2222222-2222-2222-2222-111111111111"),
            store2Id,
            catAudioId,
            "Навушники бездротові Sony WH-1000XM5 Black",
            "SONY-WH1000XM5-BLK",
            13999.00m,
            30,
            "seed"
        );
        p5.SetDetail(new ProductDetail(
            p5.Id,
            "{\"brand\": \"Sony\", \"wireless\": \"true\", \"anc\": \"true\", \"battery_life_hours\": \"30\", \"bluetooth\": \"5.2\"}",
            "Бездротові навушники з провідною технологією активного шумозаглушення ANC.",
            "seed"
        ));

        var p6 = new Product(
            Guid.Parse("d2222222-2222-2222-2222-222222222222"),
            store2Id,
            catPeripheralsId,
            "Механічна клавіатура Keychron K2 Pro Wireless",
            "KEYCHRON-K2PRO-RED",
            4500.00m,
            50,
            "seed"
        );
        p6.SetDetail(new ProductDetail(
            p6.Id,
            "{\"brand\": \"Keychron\", \"layout\": \"75%\", \"switches\": \"K Pro Red\", \"hotswap\": \"true\", \"backlight\": \"RGB\"}",
            "Преміальна бездротова механічна клавіатура з перемикачами з можливістю гарячої заміни.",
            "seed"
        ));

        await context.Products.AddRangeAsync(new[] { p1, p2, p3, p4, p5, p6 }, ct);

        // 3. Product Images (1:N)
        var images = new List<ProductImage>
        {
            new(Guid.Parse("fa011111-1111-1111-1111-111111111111"), p1.Id, "https://cdn.example.com/products/baccarat-rouge-540-front.jpg", 0, "seed"),
            new(Guid.Parse("fa011111-1111-1111-1111-222222222222"), p1.Id, "https://cdn.example.com/products/baccarat-rouge-540-box.jpg", 1, "seed"),
            new(Guid.Parse("fa011111-1111-1111-1111-333333333333"), p2.Id, "https://cdn.example.com/products/dior-sauvage-edp.jpg", 0, "seed"),
            new(Guid.Parse("fa022222-2222-2222-2222-111111111111"), p5.Id, "https://cdn.example.com/products/sony-wh1000xm5-side.jpg", 0, "seed"),
            new(Guid.Parse("fa022222-2222-2222-2222-222222222222"), p6.Id, "https://cdn.example.com/products/keychron-k2pro-top.jpg", 0, "seed")
        };
        await context.ProductImages.AddRangeAsync(images, ct);

        await context.SaveChangesAsync(ct);
    }
}
