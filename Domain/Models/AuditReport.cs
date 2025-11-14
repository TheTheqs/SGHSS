//Domain/Models/AuditReport.cs

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um relatório de auditoria que resume atividades de usuários ou eventos do sistema para fins de
    /// revisão e conformidade.
    /// </summary>
    /// <remarks>Um AuditReport consolida dados relevantes de auditoria, incluindo o horário de geração, o escopo
    /// do relatório e o administrador responsável por sua criação. Esse modelo é normalmente utilizado para apoiar
    /// revisões de segurança, verificações de conformidade e supervisão operacional.</remarks>
    public class AuditReport
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string ReportDetails { get; set; } = string.Empty;

        // Relacionamentos
        public Administrator CreatedBy { get; set; } = null!;
        // Construtor padrão
        public AuditReport() { }
    }
}
