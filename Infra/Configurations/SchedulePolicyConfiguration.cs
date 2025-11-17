// Infra/Configurations/SchedulePolicyConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade SchedulePolicy.
    /// </summary>
    /// <remarks>
    /// Mapeia os campos do Value Object, configura o relacionamento um-para-um
    /// com ProfessionalSchedule e o relacionamento um-para-muitos com WeeklyWindow.
    /// </remarks>

    public class SchedulePolicyConfiguration : IEntityTypeConfiguration<SchedulePolicy>
    {
        public void Configure(EntityTypeBuilder<SchedulePolicy> builder)
        {
            builder.ToTable("SchedulePolicies");

            builder.HasKey(sp => sp.Id);

            builder.Property(sp => sp.DurationInMinutes)
                   .IsRequired();

            // VO: TimeZone
            builder.Property(sp => sp.TimeZone)
                   .IsRequired()
                   .HasConversion(
                        tz => tz.Value,
                        value => new Domain.ValueObjects.TimeZone(value)
                    )
                   .HasMaxLength(100);

            // Relacionamentos

            builder.HasOne(sp => sp.ProfessionalSchedule)
                   .WithOne(ps => ps.SchedulePolicy)
                   .HasForeignKey<SchedulePolicy>("ProfessionalScheduleId");

            builder.HasMany(sp => sp.WeeklyWindows)
                   .WithOne(ww => ww.SchedulePolicy);
        }
    }
}
