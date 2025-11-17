// Infra/Configurations/ConsentConfiguration.cs


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade ScheduleSlot.
    /// </summary>
    /// <remarks>
    /// Define o mapeamento de propriedades e os relacionamentos referentes aos intervalos
    /// de agendamento, incluindo o vínculo opcional com Appointment e a associação obrigatória
    /// com a agenda do profissional.
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
                   .IsRequired(false)
                   .HasForeignKey<ScheduleSlot>("AppointmentId");
        }
    }
}
