// Infra/Configurations/EletronicPrescriptionConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade EletronicPrescription.
    /// </summary>
    /// <remarks>
    /// Define o mapeamento das propriedades básicas, do value object IcpSignature
    /// e dos relacionamentos com Appointment, Patient, Professional e HealthUnit.
    /// Utiliza convenções de shadow FKs para manter o domínio limpo e livre de chaves explícitas.
    /// </remarks>
    public class EletronicPrescriptionConfiguration
        : IEntityTypeConfiguration<EletronicPrescription>
    {
        public void Configure(EntityTypeBuilder<EletronicPrescription> builder)
        {
            builder.ToTable("EletronicPrescriptions");

            builder.HasKey(ep => ep.Id);

            builder.Property(ep => ep.CreatedAt)
                   .IsRequired();

            builder.Property(ep => ep.ValidUntil)
                   .IsRequired();

            builder.Property(ep => ep.Instructions)
                   .HasMaxLength(2000);

            // Value Object: IcpSignature
            builder.Property(ep => ep.IcpSignature)
                   .IsRequired()
                   .HasConversion(
                        sig => sig.Value,
                        value => new IcpSignature(value)
                   )
                   .HasColumnType("text");

            // Relacionamentos

            builder.HasOne(ep => ep.Appointment)
                   .WithOne(a => a.EletronicPrescription);

            builder.HasOne(ep => ep.Patient)
                   .WithMany(p => p.EletronicPrescriptions);

            builder.HasOne(ep => ep.Professional)
                   .WithMany(pr => pr.EletronicPrescriptions);

            builder.HasOne(ep => ep.HealthUnit)
                   .WithMany(hu => hu.EletronicPrescriptions);
        }
    }
}
