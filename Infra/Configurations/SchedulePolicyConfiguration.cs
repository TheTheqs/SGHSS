// Infra/Configurations/SchedulePolicyConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Provides the Entity Framework Core configuration for the SchedulePolicy entity.
    /// </summary>
    /// <remarks>
    /// Maps value object fields, configures the one-to-one relationship with
    /// ProfessionalSchedule, and the one-to-many relationship with WeeklyWindow.
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
                   .WithOne(ps => ps.SchedulePolicy);

            builder.HasMany(sp => sp.WeeklyWindows)
                   .WithOne(ww => ww.SchedulePolicy);
        }
    }
}
