// Application/UseCases/HomeCares/Read/HomeCareDto.cs

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa um registro simplificado de atendimento de home care
    /// para fins de consulta.
    /// </summary>
    public sealed class HomeCareDto
    {
        /// <summary>
        /// Identificador único do atendimento de home care.
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Data e horário em que o atendimento domiciliar foi realizado.
        /// </summary>
        public DateTimeOffset Date { get; init; }

        /// <summary>
        /// Descrição resumida do atendimento realizado.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Identificador do profissional responsável pelo atendimento.
        /// </summary>
        public Guid ProfessionalId { get; init; }

        /// <summary>
        /// Nome do profissional responsável pelo atendimento.
        /// </summary>
        public string ProfessionalName { get; init; } = string.Empty;

        /// <summary>
        /// Identificador da unidade de saúde responsável pelo serviço.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Nome da unidade de saúde responsável pelo serviço.
        /// </summary>
        public string HealthUnitName { get; init; } = string.Empty;
    }
}
