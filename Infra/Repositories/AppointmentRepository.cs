// Infra/Repositories/AppointmentRepository.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável pelo acesso a dados de consultas (appointments).
    /// </summary>
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly SGHSSDbContext _context;

        public AppointmentRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
        }
    }
}
