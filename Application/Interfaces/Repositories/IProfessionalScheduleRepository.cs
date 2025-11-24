// Application/Interfaces/Repositories/IProfessionalScheduleRepository.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define operações de acesso a dados relacionadas à agenda profissional,
    /// incluindo o carregamento de políticas e slots associados.
    /// </summary>
    public interface IProfessionalScheduleRepository
    {
        /// <summary>
        /// Obtém a agenda de um profissional, incluindo a SchedulePolicy,
        /// suas WeeklyWindows e os ScheduleSlots no intervalo informado.
        /// </summary>
        /// <param name="professionalId">Identificador do profissional.</param>
        /// <param name="from">Data e hora inicial (inclusiva) do intervalo de interesse.</param>
        /// <param name="to">Data e hora final (exclusiva) do intervalo de interesse.</param>
        /// <returns>
        /// A agenda profissional correspondente ou <c>null</c> caso não exista.
        /// </returns>
        Task<ProfessionalSchedule?> GetByProfessionalIdWithPolicyAndSlotsAsync(
            Guid professionalId,
            DateTime from,
            DateTime to);
    }
}
