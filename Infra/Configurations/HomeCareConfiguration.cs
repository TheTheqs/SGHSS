// Infra/Configurations/ConsentConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade HomeCare.
    /// </summary>
    /// <remarks>
    /// Define o mapeamento das propriedades básicas e dos relacionamentos com
    /// Patient, Professional e HealthUnit, utilizando navegações sem chaves
    /// estrangeiras explícitas.
    /// </remarks>
    public class HomeCareConfiguration : IEntityTypeConfiguration<HomeCare>
    {
        public void Configure(EntityTypeBuilder<HomeCare> builder)
        {
            builder.ToTable("HomeCares");

            builder.HasKey(hc => hc.Id);

            builder.Property(hc => hc.Date)
                   .IsRequired();

            builder.Property(hc => hc.Description)
                   .HasMaxLength(1000);

            // Relacionamentos

            builder.HasOne(hc => hc.Patient)
                   .WithMany(p => p.HomeCares);

            builder.HasOne(hc => hc.Professional)
                   .WithMany(pr => pr.HomeCares);

            builder.HasOne(hc => hc.HealthUnit)
                   .WithMany(hu => hu.HomeCares);
        }
    }
}
