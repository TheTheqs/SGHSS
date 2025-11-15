// Infra/Configurations/ConsentConfiguration.cs


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Provides the Entity Framework Core configuration for the ScheduleSlot entity.
    /// </summary>
    /// <remarks>
    /// Defines property mappings and relationships for schedule slot intervals,
    /// including optional appointment linkage and the required association with
    /// the professional schedule.
    /// </remarks>
    public class ScheduleSlotConfiguration : IEntityTypeConfiguration<ScheduleSlot>
    {
        public void Configure(EntityTypeBuilder<ScheduleSlot> builder)
        {
            builder.ToTable("ScheduleSlots");

            builder.HasKey(ss => ss.Id);

            builder.Property(ss => ss.StartDateTime)
                   .IsRequired();

            builder.Property(ss => ss.EndDateTime)
                   .IsRequired();

            // Enum como string
            builder.Property(ss => ss.Status)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // Relacionamentos

            builder.HasOne(ss => ss.ProfessionalSchedule)
                   .WithMany(ps => ps.ScheduleSlots);

            builder.HasOne(ss => ss.Appointment)
                   .WithOne(a => a.ScheduleSlot)
                   .IsRequired(false);
        }
    }
}
