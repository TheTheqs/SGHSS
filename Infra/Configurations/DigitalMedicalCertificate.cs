// Infra/Configurations/DigitalMedicalCertificate.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade DigitalMedicalCertificate.
    /// </summary>
    /// <remarks>
    /// Esta configuração define o mapeamento de propriedades simples, value objects e
    /// relacionamentos obrigatórios com entidades como Appointment, Patient, Professional e HealthUnit.
    /// Utiliza navegações sem chaves estrangeiras explícitas, permitindo shadow FKs geradas por convenção.
    /// </remarks>
    public class DigitalMedicalCertificateConfiguration
        : IEntityTypeConfiguration<DigitalMedicalCertificate>
    {
        public void Configure(EntityTypeBuilder<DigitalMedicalCertificate> builder)
        {
            builder.ToTable("DigitalMedicalCertificates");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.IssuedAt)
                   .IsRequired();

            builder.Property(d => d.ValidUntil)
                   .IsRequired();

            builder.Property(d => d.Recommendations)
                   .HasMaxLength(2000);

            // Value Object: IcpSignature
            builder.Property(d => d.IcpSignature)
                   .IsRequired()
                   .HasConversion(
                        sig => sig.Value,
                        value => new IcpSignature(value)
                    )
                   .HasColumnType("text");

            // Relacionamentos

            builder.HasOne(d => d.Appointment)
                   .WithOne(a => a.DigitalMedicalCertificate);

            builder.HasOne(d => d.Patient)
                   .WithMany(p => p.DigitalMedicalCertificates);

            builder.HasOne(d => d.Professional)
                   .WithMany(pr => pr.DigitalMedicalCertificates);

            builder.HasOne(d => d.HealthUnit)
                   .WithMany(hu => hu.DigitalMedicalCertificates);
        }
    }
}
