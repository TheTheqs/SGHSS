// Application/UseCases/Professionals/Update/UpdateProfessionalSchedulePolicyResponse.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Professionals.Update
{
    /// <summary>
    /// Representa os dados retornados após a atualização da política
    /// de agendamento de um profissional.
    /// </summary>
    /// <remarks>
    /// Este DTO confirma o identificador do profissional afetado e
    /// a política de agenda atualmente configurada para ele.
    /// </remarks>
    public sealed class UpdateProfessionalSchedulePolicyResponse
    {
        /// <summary>
        /// Identificador único do profissional cuja política foi atualizada.
        /// </summary>
        public Guid ProfessionalId { get; init; }

        /// <summary>
        /// Política de agendamento atualmente associada ao profissional.
        /// </summary>
        public SchedulePolicyDto SchedulePolicy { get; init; } = null!;
    }
}
