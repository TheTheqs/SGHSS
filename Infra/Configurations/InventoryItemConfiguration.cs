// Infra/Configurations/ItemInventory.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Provides the Entity Framework Core configuration for the InventoryItem entity type.
    /// </summary>
    /// <remarks>
    /// Defines property mappings, value object conversions, and relationships for InventoryItem,
    /// including its association with HealthUnit and InventoryMovement records.
    /// </remarks>
    public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
    {
        public void Configure(EntityTypeBuilder<InventoryItem> builder)
        {
            builder.ToTable("InventoryItems");

            builder.HasKey(ii => ii.Id);

            builder.Property(ii => ii.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(ii => ii.Description)
                   .HasMaxLength(1000);

            // Value Object: UnitOfMeasure
            builder.Property(ii => ii.UnitOfMeasure)
                   .IsRequired()
                   .HasConversion(
                        uom => uom.Value,
                        value => new UnitOfMeasure(value)
                    )
                   .HasMaxLength(50);

            builder.Property(ii => ii.StockQuantity)
                   .IsRequired();

            // Relacionamentos

            builder.HasOne(ii => ii.HealthUnit)
                   .WithMany(hu => hu.InventoryItems);

            builder.HasMany(ii => ii.InventoryMovement)
                   .WithOne(im => im.InventoryItem);
        }
    }
}

