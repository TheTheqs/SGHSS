// Infra/Configurations/WeeklyWindowConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade WeeklyWindow.
    /// </summary>
    /// <remarks>
    /// Mapeia propriedades relacionadas a janelas semanais recorrentes
    /// e define o relacionamento obrigatório com SchedulePolicy.
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
