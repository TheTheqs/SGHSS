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
        public AppointmentRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persiste uma nova consulta no banco de dados.
        /// </summary>
        public async Task AddAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Atualiza uma consulta existente no banco de dados.
        /// </summary>
        public async Task UpdateAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Recupera uma consulta pelo seu identificador único.
        /// </summary>
        public async Task<Appointment?> GetByIdAsync(Guid appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.ScheduleSlot)
                .Include(a => a.Patient)
                .Include(a => a.DigitalMedicalCertificate)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }

        /// <summary>
        /// Retorna todas as consultas associadas a um paciente específico.
        /// </summary>
        /// <param name="patientId">Identificador do paciente cujas consultas serão recuperadas.</param>
        /// <returns>
        /// Uma lista contendo todas as instâncias de <see cref="Appointment"/>
        /// vinculadas ao paciente informado. Caso o paciente não possua consultas,
        /// a lista retornada será vazia.
        /// </returns>
        public async Task<List<Appointment>> GetAllByPatientIdAsync(Guid patientId)
        {
            return await _context.Appointments
                .Include(a => a.ScheduleSlot)
                .Include(a => a.DigitalMedicalCertificate)
                .Where(a => a.Patient != null && a.Patient.Id == patientId)
                .OrderBy(a => a.ScheduleSlot.StartDateTime)
                .ToListAsync();
        }
    }
}
