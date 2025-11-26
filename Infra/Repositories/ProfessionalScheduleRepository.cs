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
            // - Slots associados
            //
            // Observação: o filtro de intervalo [from, to) deverá ser aplicado
            // na camada de aplicação para maior flexibilidade.
            return await _context.ProfessionalSchedules
                .Include(ps => ps.Professional)
                .Include(ps => ps.SchedulePolicy)
                    .ThenInclude(sp => sp.WeeklyWindows)
                .Include(ps => ps.ScheduleSlots)
                .FirstOrDefaultAsync(ps => ps.Professional.Id == professionalId);
        }

        /// <inheritdoc />
        public async Task<ProfessionalSchedule?> GetByIdWithSlotsAsync(Guid professionalScheduleId)
        {
            // Carrega:
            // - Agenda profissional
            // - Slots associados (todos)
            // - SchedulePolicy e WeeklyWindows
            //
            // A ideia é garantir que todos os dados necessários para
            // consultas de horários reservados estejam disponíveis.
            return await _context.ProfessionalSchedules
                .Include(ps => ps.SchedulePolicy)
                    .ThenInclude(sp => sp.WeeklyWindows)
                .Include(ps => ps.ScheduleSlots)
                .FirstOrDefaultAsync(ps => ps.Id == professionalScheduleId);
        }
    }
}
