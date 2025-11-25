// Infra/Repositories/AppointmentRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Implementação concreta do repositório responsável pelo acesso
    /// e persistência de consultas médicas (appointments).
    /// </summary>
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do repositório de consultas.
        /// </summary>
        /// <param name="context">
        /// Contexto do Entity Framework Core utilizado para acesso ao banco de dados.
        /// </param>
        public AppointmentRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persiste uma nova consulta no banco de dados.
        /// </summary>
        /// <param name="appointment">
        /// Instância de <see cref="Appointment"/> já validada e pronta para persistência.
        /// </param>
        /// <remarks>
        /// Este método executa o <c>AddAsync</c> seguido de <c>SaveChangesAsync</c>,
        /// garantindo gravação imediata.
        /// </remarks>
        public async Task AddAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza uma consulta existente no banco de dados.
        /// </summary>
        /// <param name="appointment">
        /// Instância de <see cref="Appointment"/> contendo as alterações
        /// a serem persistidas.
        /// </param>
        /// <remarks>
        /// A entidade é marcada como modificada no contexto e,
        /// em seguida, as alterações são gravadas por meio de
        /// <c>SaveChangesAsync</c>.
        /// </remarks>
        public async Task UpdateAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Recupera uma consulta pelo seu identificador único.
        /// </summary>
        /// <param name="appointmentId">ID da consulta desejada.</param>
        /// <returns>
        /// A entidade <see cref="Appointment"/> correspondente ao ID informado,
        /// ou <c>null</c> se nenhuma consulta for encontrada.
        /// </returns>
        public async Task<Appointment?> GetByIdAsync(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.ScheduleSlot)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }
    }
}
