// Infra/Configurations/ProfessionalScheduleConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade ProfessionalSchedule.
    /// </summary>
    /// <remarks>
    /// Define o mapeamento dos relacionamentos entre ProfessionalSchedule,
    /// Professional, SchedulePolicy e ScheduleSlot.
    /// </remarks>

    public class ProfessionalScheduleConfiguration : IEntityTypeConfiguration<ProfessionalSchedule>
    {
        public void Configure(EntityTypeBuilder<ProfessionalSchedule> builder)
        {
            builder.ToTable("ProfessionalSchedules");

            builder.HasKey(ps => ps.Id);

            // Relacionamentos

            builder.HasOne(ps => ps.Professional)
                   .WithOne(p => p.ProfessionalSchedule);

            builder.HasOne(ps => ps.SchedulePolicy)
                   .WithOne(sp => sp.ProfessionalSchedule);

            builder.HasMany(ps => ps.ScheduleSlots)
                   .WithOne(ss => ss.ProfessionalSchedule);
        }
    }
}
