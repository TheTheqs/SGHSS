// Application/UseCases/HomeCares/Register/RegisterHomeCareRequest.cs

namespace SGHSS.Application.UseCases.HomeCares.Register
{
    /// <summary>
    /// Representa a solicitação para registrar um novo atendimento
    /// de home care para um paciente.
    /// </summary>
    /// <remarks>
    /// Este request reúne as informações necessárias para criação
    /// de um registro de home care, incluindo o paciente atendido,
    /// o profissional responsável, a unidade de saúde e detalhes
    /// descritivos do atendimento.
    /// </remarks>
    public sealed class RegisterHomeCareRequest
    {
        /// <summary>
        /// Data e horário em que o atendimento de home care foi realizado.
        /// </summary>
        public DateTimeOffset Date { get; init; }

        /// <summary>
        /// Descrição do atendimento realizado, incluindo observações
        /// relevantes sobre o estado do paciente ou procedimentos executados.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Identificador único do paciente que recebeu o atendimento
        /// de home care.
        /// </summary>
        public Guid PatientId { get; init; }

        /// <summary>
        /// Identificador único do profissional que realizou o
        /// atendimento de home care.
        /// </summary>
        public Guid ProfessionalId { get; init; }

        /// <summary>
        /// Identificador único da unidade de saúde responsável
        /// pelo serviço de home care.
        /// </summary>
        public Guid HealthUnitId { get; init; }
    }
}
