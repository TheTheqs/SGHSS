// Infra/Configurations/AuditReportConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade AuditReport.
    /// </summary>
    /// <remarks>
    /// Esta configuração define o mapeamento das propriedades básicas e o relacionamento
    /// com a entidade Administrator, utilizando navegações sem chaves estrangeiras explícitas.
    /// O EF Core cria automaticamente a shadow FK correspondente.
    /// </remarks>
    public class AuditReportConfiguration : IEntityTypeConfiguration<AuditReport>
    {
        public void Configure(EntityTypeBuilder<AuditReport> builder)
        {
            builder.ToTable("AuditReports");

            builder.HasKey(ar => ar.Id);

            builder.Property(ar => ar.CreatedAt)
                   .IsRequired();

            builder.Property(ar => ar.ReportDetails)
                   .IsRequired()
                   .HasMaxLength(2000);

            // Relacionamento: muitos relatórios -> um Administrador
            builder.HasOne(ar => ar.CreatedBy)
                   .WithMany(a => a.AuditReports);
        }
    }
}
