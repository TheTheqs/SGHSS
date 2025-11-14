// Domain/Models/FinancialReport.cs

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um relatório financeiro contendo informações de receita, despesas e lucro para um período específico.
    /// </summary>
    /// <remarks>Um FinancialReport agrega métricas financeiras essenciais e geralmente está associado a um
    /// administrador responsável pelo relatório. A data do relatório indica o período ao qual os dados
    /// financeiros se aplicam.</remarks>
    public class FinancialReport
    {
        public Guid Id { get; set; }
        public DateTimeOffset ReportDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit => TotalRevenue - TotalExpenses;

        // Relacionamentos
        public Administrator Administrator { get; set; } = null!;

        // Construtor padrão
        public FinancialReport() { }
    }
}
