// Infra/Configurations/AppointmentConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade Appointment.
    /// </summary>
    /// <remarks>
    /// Esta configuração define o mapeamento das propriedades de consulta, incluindo datas,
    /// enums, value objects e todos os relacionamentos associados.  
    /// O EF Core utiliza convenções para gerar chaves estrangeiras automáticas (shadow FKs),
    /// permitindo um modelo de domínio mais limpo e independente.
    /// </remarks>
    
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            // Nome da tabela
            builder.ToTable("Appointments");

            builder.HasKey(a => a.Id);

            // Propriedades simples
            builder.Property(a => a.StartTime)
                   .IsRequired();

            builder.Property(a => a.EndTime)
                   .IsRequired();

            // Enums como string
            builder.Property(a => a.Status)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(a => a.Type)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // VO: Link (nullable)
            builder.Property(a => a.Link)
                   .HasConversion(
                        link => link.HasValue ? link.Value.Value : null,
                        value => value == null ? (Link?)null : new Link(value)
                    )
                   .HasMaxLength(200)
                   .IsRequired(false);

            builder.Property(a => a.Description)
                   .HasMaxLength(500);

            // Relacionamentos
            builder.HasOne(a => a.Patient)
                   .WithMany(p => p.Appointments);
        }
    }
}
