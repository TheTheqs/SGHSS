// Domain/Models/Administrator.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um usuário administrador com permissões elevadas no sistema, incluindo nível de acesso e
    /// associações com movimentos de estoque e relatórios gerados.
    /// </summary>
    /// <remarks>Um Administrator estende a classe base User adicionando privilégios administrativos por meio
    /// de uma classificação de nível de acesso. A classe também mantém referências a movimentos de estoque,
    /// bem como relatórios de auditoria e financeiros criados ou gerenciados pelo administrador.</remarks>


    public class Administrator : User
    {
        public AccessLevel AccessLevel { get; set; }

        // Relacionamentos
        public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
        public ICollection<AuditReport> AuditReports { get; set; } = new List<AuditReport>();
        public ICollection<FinancialReport> FinancialReports { get; set; } = new List<FinancialReport>();

        // Construtor padrão
        public Administrator() { }
    }
}
