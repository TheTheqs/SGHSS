// Application/UseCases/Professionals/Update/UpdateProfessionalSchedulePolicyRequest.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Professionals.Update
{
    /// <summary>
    /// Representa os dados de entrada necessários para atualizar
    /// a política de agendamento de um profissional.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado pela camada de Application para receber o
    /// identificador do profissional e a nova política de agenda a ser aplicada,
    /// sem expor diretamente os modelos de domínio.
    /// </remarks>
    public sealed class UpdateProfessionalSchedulePolicyRequest
    {
        /// <summary>
        /// Identificador único do profissional cuja política de agenda será atualizada.
        /// </summary>
        public Guid ProfessionalId { get; init; }

        /// <summary>
        /// Nova política de agendamento a ser aplicada ao profissional.
        /// </summary>
        public SchedulePolicyDto SchedulePolicy { get; init; } = null!;
    }
}
