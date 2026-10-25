using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Dal.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasColumnName("full_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.Phone)
            .HasColumnName("phone")
            .HasMaxLength(30);

        builder.Property(u => u.SystemRole)
            .HasColumnName("system_role")
            .HasMaxLength(50)
            .HasDefaultValue("Merchant")
            .IsRequired();

        builder.Property(u => u.Version)
            .HasColumnName("version")
            .HasDefaultValue(1)
            .IsConcurrencyToken();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .HasDefaultValue("system")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(u => u.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        builder.Property(u => u.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .HasDatabaseName("uq_users_email_active")
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_users_phone", "phone IS NULL OR phone ~ '^\\+[1-9][0-9]{7,14}$'");
            t.HasCheckConstraint("ck_users_system_role", "system_role IN ('PlatformAdmin', 'Merchant', 'Staff')");
        });

        builder.HasData(
            new
            {
                Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                Email = "owner.parfums@example.com",
                PasswordHash = "$2a$11$e8.Z/yY5tW9uGqL1vH5jEOzR9q4bVxY4oJ7d8v7f7x9m9q9w9e9r9",
                FullName = "Богдан Рибонька",
                Phone = "+380509998877",
                SystemRole = "Merchant",
                Version = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "seed",
                IsDeleted = false
            },
            new
            {
                Id = Guid.Parse("a2222222-2222-2222-2222-111111111111"),
                Email = "owner.tech@example.com",
                PasswordHash = "$2a$11$e8.Z/yY5tW9uGqL1vH5jEOzR9q4bVxY4oJ7d8v7f7x9m9q9w9e9r9",
                FullName = "Олександр Петренко",
                Phone = "+380679998877",
                SystemRole = "Merchant",
                Version = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "seed",
                IsDeleted = false
            },
            new
            {
                Id = Guid.Parse("a1111111-1111-1111-1111-222222222222"),
                Email = "manager.parfums@example.com",
                PasswordHash = "$2a$11$e8.Z/yY5tW9uGqL1vH5jEOzR9q4bVxY4oJ7d8v7f7x9m9q9w9e9r9",
                FullName = "Тетяна Дмитрук",
                Phone = "+380939998877",
                SystemRole = "Staff",
                Version = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedBy = "seed",
                IsDeleted = false
            }
        );
    }
}
