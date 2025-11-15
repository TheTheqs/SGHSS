// Infra/Configurations/WeeklyWindowConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Provides the Entity Framework Core configuration for the WeeklyWindow entity.
    /// </summary>
    /// <remarks>
    /// Maps properties related to weekly recurring schedule windows and defines
    /// the required relationship with SchedulePolicy.
    /// </remarks>
    public class WeeklyWindowConfiguration : IEntityTypeConfiguration<WeeklyWindow>
    {
        public void Configure(EntityTypeBuilder<WeeklyWindow> builder)
        {
            builder.ToTable("WeeklyWindows");

            builder.HasKey(ww => ww.Id);

            builder.Property(ww => ww.DayOfWeek)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(ww => ww.StartTime)
                   .HasColumnType("time")
                   .IsRequired();

            builder.Property(ww => ww.EndTime)
                   .HasColumnType("time")
                   .IsRequired();

            // Relacionamentos

            builder.HasOne(ww => ww.SchedulePolicy)
                   .WithMany(sp => sp.WeeklyWindows)
                   .IsRequired();
        }
    }
}
