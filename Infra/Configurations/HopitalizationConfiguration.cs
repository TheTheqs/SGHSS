// Infra/Configurations/HospitalizationConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade Hospitalization.
    /// </summary>
    /// <remarks>
    /// Define o mapeamento das propriedades básicas e dos relacionamentos com Patient e Bed,
    /// incluindo a conversão do enum HospitalizationStatus para string.
    /// </remarks>
    public class HospitalizationConfiguration : IEntityTypeConfiguration<Hospitalization>
    {
        public void Configure(EntityTypeBuilder<Hospitalization> builder)
        {
            builder.ToTable("Hospitalizations");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.AdmissionDate)
                   .IsRequired();

            builder.Property(h => h.DischargeDate);

            builder.Property(h => h.Reason)
                   .HasMaxLength(1000);

            builder.Property(h => h.Status)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // Relacionamentos

            builder.HasOne(h => h.Patient)
                   .WithMany(p => p.Hospitalizations);

            builder.HasOne(h => h.Bed)
                   .WithOne(b => b.CurrentHospitalization)
                   .HasForeignKey<Bed>("AppointmentId");
        }
    }
}
