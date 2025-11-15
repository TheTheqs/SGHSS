// Infra/Configurations/HealthUnitConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;
using System.Text.Json;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade HealthUnit.
    /// </summary>
    /// <remarks>
    /// Define o mapeamento de propriedades básicas, value objects e relacionamentos
    /// com pacientes, profissionais, recursos de infraestrutura e registros clínicos
    /// associados à unidade de saúde.
    /// </remarks>
    public class HealthUnitConfiguration : IEntityTypeConfiguration<HealthUnit>
    {
        public void Configure(EntityTypeBuilder<HealthUnit> builder)
        {
            builder.ToTable("HealthUnits");

            builder.HasKey(hu => hu.Id);

            builder.Property(hu => hu.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            // VO: Cnpj
            builder.Property(hu => hu.Cnpj)
                   .IsRequired()
                   .HasConversion(
                        cnpj => cnpj.Value,
                        value => new Cnpj(value)
                    )
                   .HasMaxLength(14);

            // VO: Address (composto) armazenado como JSON
            builder.Property(hu => hu.Address)
                   .IsRequired()
                   .HasConversion(
                        addr => JsonSerializer.Serialize(addr, (JsonSerializerOptions?)null),
                        json => JsonSerializer.Deserialize<Address>(json, (JsonSerializerOptions?)null)!
                    )
                   .HasColumnType("text");

            // VO: Phone
            builder.Property(hu => hu.Phone)
                   .IsRequired()
                   .HasConversion(
                        phone => phone.Value,
                        value => new Phone(value)
                    )
                   .HasMaxLength(20);

            // Enum: HealthUnitType como string
            builder.Property(hu => hu.Type)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // Relacionamentos

            builder.HasMany(hu => hu.Patients)
                   .WithMany(p => p.HealthUnits);

            builder.HasMany(hu => hu.Professionals)
                   .WithMany(p => p.HealthUnits);

            builder.HasMany(hu => hu.Appointments)
                   .WithOne();

            builder.HasMany(hu => hu.MedicalRecordUpdates)
                   .WithOne(mru => mru.HealthUnit);

            builder.HasMany(hu => hu.EletronicPrescriptions)
                   .WithOne(ep => ep.HealthUnit);

            builder.HasMany(hu => hu.DigitalMedicalCertificates)
                   .WithOne(dmc => dmc.HealthUnit);

            builder.HasMany(hu => hu.HomeCares)
                   .WithOne(hc => hc.HealthUnit);

            builder.HasMany(hu => hu.InventoryItems)
                   .WithOne(ii => ii.HealthUnit);

            builder.HasMany(hu => hu.Beds)
                   .WithOne(b => b.HealthUnit);

            // 1:N HealthUnit -> LogActivities
            builder.HasMany(hu => hu.LogActivities)
                   .WithOne(la => la.HealthUnit);
        }
    }
}
