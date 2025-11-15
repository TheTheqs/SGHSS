// Infra/Configurations/MedicalRecordConfigural.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Provides the Entity Framework Core configuration for the MedicalRecord entity.
    /// </summary>
    /// <remarks>
    /// Defines mappings for the MedicalRecord value object number, creation date,
    /// and relationships with Patient and MedicalRecordUpdate entries.
    /// </remarks>
    public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.ToTable("MedicalRecords");

            builder.HasKey(mr => mr.Id);

            builder.Property(mr => mr.CreatedAt)
                   .IsRequired();

            // VO: MedicalRecordNumber
            builder.Property(mr => mr.Number)
                   .IsRequired()
                   .HasConversion(
                        num => num.Value,
                        value => new MedicalRecordNumber(value))
                   .HasMaxLength(50);

            // RELACIONAMENTOS
            builder.HasOne(mr => mr.Patient)
                   .WithOne(p => p.MedicalRecord);

            // 1 : N → MedicalRecordUpdate
            builder.HasMany(mr => mr.Updates)
                   .WithOne(u => u.MedicalRecord);
        }
    }
}
