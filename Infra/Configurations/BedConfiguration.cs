// Infra/Configurations/BedConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade Bed.
    /// </summary>
    /// <remarks>
    /// Esta configuração define o mapeamento das propriedades básicas, enums e
    /// relacionamentos relacionados ao gerenciamento de leitos hospitalares.
    /// Inclui o vínculo com a unidade de saúde e a internação atual, utilizando
    /// navegações sem chaves estrangeiras explícitas.
    /// </remarks>
    public class BedConfiguration : IEntityTypeConfiguration<Bed>
    {
        public void Configure(EntityTypeBuilder<Bed> builder)
        {
            builder.ToTable("Beds");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.BedNumber)
                   .IsRequired()
                   .HasMaxLength(50);

            // Enums como string
            builder.Property(b => b.Type)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(b => b.Status)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // Relacionamentos
            builder.HasOne(b => b.HealthUnit)
                   .WithMany(hu => hu.Beds);
        }
    }
}
