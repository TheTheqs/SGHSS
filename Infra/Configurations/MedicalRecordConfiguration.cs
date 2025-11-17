// Infra/Configurations/MedicalRecordConfigural.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade MedicalRecord.
    /// </summary>
    /// <remarks>
    /// Define o mapeamento para o Value Object Number, data de criação
    /// e os relacionamentos com Patient e MedicalRecordUpdate.
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
                   .WithOne(p => p.MedicalRecord)
                   .HasForeignKey<MedicalRecord>("PatientId");

            builder.HasMany(mr => mr.Updates)
                   .WithOne(u => u.MedicalRecord);
        }
    }
}
