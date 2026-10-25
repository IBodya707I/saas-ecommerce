using Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Dal.Configurations;

public class StoreEmployeeConfiguration : IEntityTypeConfiguration<StoreEmployee>
{
    public void Configure(EntityTypeBuilder<StoreEmployee> builder)
    {
        builder.ToTable("store_employees");

        builder.HasKey(se => new { se.StoreId, se.UserId });

        builder.Property(se => se.StoreId)
            .HasColumnName("store_id")
            .IsRequired();

        builder.Property(se => se.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(se => se.Role)
            .HasColumnName("role")
            .HasMaxLength(50)
            .HasDefaultValue("Manager")
            .IsRequired();

        builder.Property(se => se.AssignedAt)
            .HasColumnName("assigned_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(se => se.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(se => se.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(se => se.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .HasDefaultValue("system")
            .IsRequired();

        builder.Property(se => se.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(se => se.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        builder.Property(se => se.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_store_employees_role", "role IN ('Owner', 'Manager', 'CatalogEditor', 'OrderOperator')");
        });

        builder.HasIndex(se => se.UserId)
            .HasDatabaseName("ix_store_employees_user_active")
            .HasFilter("is_active = true AND is_deleted = false");

        builder.HasOne(se => se.Store)
            .WithMany(s => s.StoreEmployees)
            .HasForeignKey(se => se.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(se => se.User)
            .WithMany(u => u.StoreEmployees)
            .HasForeignKey(se => se.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
