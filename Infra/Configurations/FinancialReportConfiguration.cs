// Infra/Configurations/FinancialReportConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade FinancialReport.
    /// </summary>
    /// <remarks>
    /// Esta configuração define o mapeamento de propriedades simples e o relacionamento
    /// com a entidade Administrator. A propriedade calculada NetProfit é ignorada,
    /// pois não deve ser persistida no banco de dados.
    /// </remarks>
    public class FinancialReportConfiguration : IEntityTypeConfiguration<FinancialReport>
    {
        public void Configure(EntityTypeBuilder<FinancialReport> builder)
        {
            builder.ToTable("FinancialReports");

            builder.HasKey(fr => fr.Id);

            builder.Property(fr => fr.ReportDate)
                   .IsRequired();

            builder.Property(fr => fr.TotalRevenue)
                   .HasColumnType("numeric(18,2)")
                   .IsRequired();

            builder.Property(fr => fr.TotalExpenses)
                   .HasColumnType("numeric(18,2)")
                   .IsRequired();

            // Propriedade calculada (não deve virar coluna)
            builder.Ignore(fr => fr.NetProfit);

            // Relacionamentos
            builder.HasOne(fr => fr.Administrator)
                   .WithMany(a => a.FinancialReports);
        }
    }
}
