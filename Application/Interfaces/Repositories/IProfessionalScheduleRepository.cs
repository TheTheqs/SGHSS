// Application/Interfaces/Repositories/IProfessionalScheduleRepository.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define operações de acesso a dados relacionadas à agenda profissional,
    /// permitindo o carregamento de políticas, janelas semanais e slots de horário.
    /// </summary>
    public interface IProfessionalScheduleRepository
    {
        /// <summary>
        /// Obtém a agenda de um profissional com sua <see cref="SchedulePolicy"/>,
        /// janelas semanais (<c>WeeklyWindows</c>) e todos os <see cref="ScheduleSlot"/>
        /// gerados dentro de um intervalo específico.
        /// </summary>
        /// <param name="professionalId">Identificador do profissional.</param>
        /// <param name="from">Data/hora inicial (inclusiva) do intervalo de consulta.</param>
        /// <param name="to">Data/hora final (exclusiva) do intervalo de consulta.</param>
        /// <returns>
        /// A agenda profissional correspondente, ou <c>null</c> caso não exista.
        /// </returns>
        Task<ProfessionalSchedule?> GetByProfessionalIdWithPolicyAndSlotsAsync(
            Guid professionalId,
            DateTime from,
            DateTime to);

        /// <summary>
        /// Obtém uma agenda profissional específica pelo seu identificador único,
        /// incluindo todos os <see cref="ScheduleSlot"/> associados a ela.
        /// </summary>
        /// <param name="professionalScheduleId">
        /// Identificador da agenda profissional que se deseja carregar.
        /// </param>
        /// <returns>
        /// A agenda profissional com seus slots completamente carregados,
        /// ou <c>null</c> caso não seja encontrada.
        /// </returns>
        /// <remarks>
        /// Este método é utilizado em casos de uso que precisam consultar
        /// os horários reservados ou ocupados de uma agenda específica.
        /// A implementação deve garantir o carregamento das coleções
        /// de forma explícita via <c>Include</c> / <c>ThenInclude</c>.
        /// </remarks>
        Task<ProfessionalSchedule?> GetByIdWithSlotsAsync(Guid professionalScheduleId);
    }
}
