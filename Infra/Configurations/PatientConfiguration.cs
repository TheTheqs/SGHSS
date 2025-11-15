// Infra/Configurations/PatientConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;
using System.Text.Json;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Provides the Entity Framework Core configuration for the Patient entity.
    /// </summary>
    /// <remarks>
    /// Maps specific Patient properties such as CPF, birth date, sex, address and emergency contact,
    /// as well as its relationships with appointments, medical records, prescriptions,
    /// certificates, hospitalizations and home care services.
    /// </remarks>
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {

            builder.Property(p => p.BirthDate)
                   .IsRequired();

            builder.Property(p => p.Sex)
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(p => p.EmergencyContactName)
                   .HasMaxLength(200);

            // CPF (VO)
            builder.Property(p => p.Cpf)
                   .IsRequired()
                   .HasConversion(
                        cpf => cpf.Value,
                        value => new Cpf(value))
                   .HasMaxLength(11);

            // VO: Address (composto) armazenado como JSON
            builder.Property(p => p.Address)
                   .IsRequired()
                   .HasConversion(
                        addr => JsonSerializer.Serialize(addr, (JsonSerializerOptions?)null),
                        json => JsonSerializer.Deserialize<Address>(json, (JsonSerializerOptions?)null)!
                    )
                   .HasColumnType("text");

            // Relacionamentos

            builder.HasMany(p => p.HealthUnits)
                   .WithMany(hu => hu.Patients);

            builder.HasMany(p => p.Appointments)
                   .WithOne(a => a.Patient);

            builder.HasOne(p => p.MedicalRecord)
                   .WithOne(mr => mr.Patient)
                   .IsRequired();

            builder.HasMany(p => p.EletronicPrescriptions)
                   .WithOne(ep => ep.Patient);

            builder.HasMany(p => p.DigitalMedicalCertificates)
                   .WithOne(dmc => dmc.Patient);

            builder.HasMany(p => p.Hospitalizations)
                   .WithOne(h => h.Patient);

            builder.HasMany(p => p.HomeCares)
                   .WithOne(hc => hc.Patient);
        }
    }
}
