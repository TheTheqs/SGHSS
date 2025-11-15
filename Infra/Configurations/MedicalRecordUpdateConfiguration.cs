// Infra/Configurations/MedicalRecordUpdateConfiguration.cs


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Provides the Entity Framework Core configuration for the MedicalRecordUpdate entity.
    /// </summary>
    /// <remarks>
    /// Defines mappings for basic properties and relationships with MedicalRecord,
    /// Professional, HealthUnit, and optionally Appointment.
    /// </remarks>
    public class MedicalRecordUpdateConfiguration : IEntityTypeConfiguration<MedicalRecordUpdate>
    {
        public void Configure(EntityTypeBuilder<MedicalRecordUpdate> builder)
        {
            builder.ToTable("MedicalRecordUpdates");

            builder.HasKey(mru => mru.Id);

            builder.Property(mru => mru.UpdateDate)
                   .IsRequired();

            builder.Property(mru => mru.Description)
                   .IsRequired()
                   .HasMaxLength(2000);

            // Relacionamentos

            builder.HasOne(mru => mru.MedicalRecord)
                   .WithMany(mr => mr.Updates);

            builder.HasOne(mru => mru.Professional)
                   .WithMany(p => p.MedicalRecordUpdates);

            builder.HasOne(mru => mru.HealthUnit)
                   .WithMany(hu => hu.MedicalRecordUpdates);

            builder.HasOne(mru => mru.Appointment)
                   .WithOne(a => a.MedicalRecordUpdate)
                   .IsRequired(false);
        }
    }
}
