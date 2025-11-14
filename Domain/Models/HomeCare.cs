// Domain/Models/HomeCare.cs

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um serviço de home care prestado a um paciente, incluindo detalhes sobre o paciente,
    /// o profissional, a unidade de saúde e a data do atendimento.
    /// </summary>
    /// <remarks>Use esta classe para registrar e gerenciar informações sobre visitas individuais de home care,
    /// incluindo o paciente associado, o profissional que realizou o atendimento e a unidade de saúde
    /// responsável pelo serviço.</remarks>

    public class HomeCare
    {
        public Guid Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Description { get; set; } = string.Empty;

        // Relacionamentos
        public Patient Patient { get; set; } = null!;
        public Professional Professional { get; set; } = null!;
        public HealthUnit HealthUnit { get; set; } = null!;

        // Construtor padrão
        public HomeCare() { }
    }
}
