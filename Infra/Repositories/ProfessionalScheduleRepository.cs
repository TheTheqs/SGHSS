// Infra/Repositories/ProfessionalScheduleRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável pelo acesso à agenda profissional, incluindo
    /// a política de agendamento, janelas semanais e slots associados.
    /// </summary>
    public class ProfessionalScheduleRepository : IProfessionalScheduleRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de agendas profissionais.
        /// </summary>
        /// <param name="context">Contexto de banco de dados utilizado para acesso aos dados.</param>
        public ProfessionalScheduleRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<ProfessionalSchedule?> GetByProfessionalIdWithPolicyAndSlotsAsync(
            Guid professionalId,
            DateTime from,
            DateTime to)
        {
            // Carrega:
            // - Agenda do profissional
            // - SchedulePolicy
            // - WeeklyWindows
            // - Slots associados à agenda
            //
            // O filtro de intervalo [from, to) será aplicado na camada de aplicação
            // (no UseCase), permitindo reutilização flexível deste método.
            var schedule = await _context.ProfessionalSchedules
                .Include(ps => ps.Professional)
                .Include(ps => ps.SchedulePolicy)
                    .ThenInclude(sp => sp.WeeklyWindows)
                .Include(ps => ps.ScheduleSlots)
                .FirstOrDefaultAsync(ps => ps.Professional.Id == professionalId);

            return schedule;
        }
    }
}
