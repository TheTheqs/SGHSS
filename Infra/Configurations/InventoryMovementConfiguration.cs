// Infra/Configurations/InventoryMovementConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade InventoryMovement.
    /// </summary>
    /// <remarks>
    /// Define o mapeamento de propriedades básicas, incluindo o enum de tipo de movimento,
    /// bem como os relacionamentos com InventoryItem e Administrator para rastreamento de operações de estoque.
    /// </remarks>
    public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
    {
        public void Configure(EntityTypeBuilder<InventoryMovement> builder)
        {
            builder.ToTable("InventoryMovements");

            builder.HasKey(im => im.Id);

            builder.Property(im => im.MovementDate)
                   .IsRequired();

            builder.Property(im => im.Quantity)
                   .IsRequired();

            builder.Property(im => im.Description)
                   .HasMaxLength(500);

            // Enum como string
            builder.Property(im => im.MovementType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // Relacionamentos

            builder.HasOne(im => im.InventoryItem)
                   .WithMany(ii => ii.InventoryMovement);

            builder.HasOne(im => im.Administrator)
                   .WithMany(a => a.InventoryMovements);
        }
    }
}
