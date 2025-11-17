// Infra/Configurations/ProfessionalConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade Professional.
    /// </summary>
    /// <remarks>
    /// Define o mapeamento de Value Objects, enums e todos os relacionamentos
    /// relacionados ao Professional, incluindo agenda, unidades de saúde,
    /// consultas, prescrições, atestados e atualizações de prontuário.
    /// </remarks>

    public class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
    {
        public void Configure(EntityTypeBuilder<Professional> builder)
        {

            // VO: ProfessionalLicense
            builder.Property(p => p.License)
                   .IsRequired()
                   .HasConversion(
                        lic => lic.Value,
                        value => new ProfessionalLicense(value))
                   .HasMaxLength(50);

            builder.Property(p => p.Specialty)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(p => p.Availability)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // Relacionamentos

            builder.HasMany(p => p.HealthUnits)
                   .WithMany(hu => hu.Professionals);

            builder.HasOne(p => p.ProfessionalSchedule)
                   .WithOne(ps => ps.Professional);

            builder.HasMany(p => p.MedicalRecordUpdates)
                   .WithOne(mru => mru.Professional);

            builder.HasMany(p => p.EletronicPrescriptions)
                   .WithOne(ep => ep.Professional);

            builder.HasMany(p => p.DigitalMedicalCertificates)
                   .WithOne(cert => cert.Professional);

            builder.HasMany(p => p.HomeCares)
                   .WithOne(hc => hc.Professional);
        }
    }
}
