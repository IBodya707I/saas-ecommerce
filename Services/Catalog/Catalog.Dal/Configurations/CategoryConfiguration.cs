using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Dal.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.StoreId)
            .HasColumnName("store_id")
            .IsRequired();

        builder.Property(c => c.ParentCategoryId)
            .HasColumnName("parent_category_id");

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasColumnName("slug")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Version)
            .HasColumnName("version")
            .HasDefaultValue(1)
            .IsConcurrencyToken();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .HasDefaultValue("system")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(c => c.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(c => new { c.StoreId, c.Slug })
            .HasDatabaseName("uq_categories_store_slug_active")
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_categories_not_self_parent", "parent_category_id IS NULL OR parent_category_id <> id");
        });

        builder.HasOne(c => c.Store)
            .WithMany(s => s.Categories)
            .HasForeignKey(c => c.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new
            {
                Id = Guid.Parse("ca011111-1111-1111-1111-111111111111"),
                StoreId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ParentCategoryId = (Guid?)null,
                Name = "Парфумерія",
                Slug = "parfums",
                Version = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "seed",
                IsDeleted = false
            },
            new
            {
                Id = Guid.Parse("ca011111-1111-1111-1111-222222222222"),
                StoreId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ParentCategoryId = (Guid?)Guid.Parse("ca011111-1111-1111-1111-111111111111"),
                Name = "Нішеві парфуми",
                Slug = "niche-parfums",
                Version = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "seed",
                IsDeleted = false
            },
            new
            {
                Id = Guid.Parse("ca011111-1111-1111-1111-333333333333"),
                StoreId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                ParentCategoryId = (Guid?)Guid.Parse("ca011111-1111-1111-1111-111111111111"),
                Name = "Люксова парфумерія",
                Slug = "luxury-parfums",
                Version = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "seed",
                IsDeleted = false
            },
            new
            {
                Id = Guid.Parse("ca022222-2222-2222-2222-111111111111"),
                StoreId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ParentCategoryId = (Guid?)null,
                Name = "Аудіотехніка",
                Slug = "audio",
                Version = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "seed",
                IsDeleted = false
            },
            new
            {
                Id = Guid.Parse("ca022222-2222-2222-2222-222222222222"),
                StoreId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ParentCategoryId = (Guid?)null,
                Name = "Периферія",
                Slug = "peripherals",
                Version = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "seed",
                IsDeleted = false
            }
        );
    }
}
