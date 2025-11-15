// Infra/Configurations/LogActivityConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade LogActivity.
    /// </summary>
    /// <remarks>
    /// Define o mapeamento de propriedades, conversões de Value Objects, 
    /// enum como string e relacionamentos com User e HealthUnit.
    /// </remarks>
    public class LogActivityConfiguration : IEntityTypeConfiguration<LogActivity>
    {
        public void Configure(EntityTypeBuilder<LogActivity> builder)
        {
            builder.ToTable("LogActivities");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Timestamp)
                   .IsRequired();

            builder.Property(l => l.Action)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(l => l.Description)
                   .HasMaxLength(500);

            // Enum como string
            builder.Property(l => l.Result)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // VO: IpAddress
            builder.Property(l => l.IpAddress)
                   .IsRequired()
                   .HasConversion(
                        ip => ip.Value,
                        value => new IpAddress(value)
                   )
                   .HasMaxLength(45);

            // Relacionamentos

            builder.HasOne(l => l.User)
                   .WithMany(u => u.LogActivities);

            builder.HasOne(l => l.HealthUnit)
                   .WithMany(hu => hu.LogActivities)
                   .IsRequired(false);
        }
    }
}
