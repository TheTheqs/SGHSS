// Application/Interfaces/Repositories/IAppointmentRepository.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define operações de acesso a dados relacionadas a consultas (appointments).
    /// </summary>
    public interface IAppointmentRepository
    {
        /// <summary>
        /// Persiste uma nova consulta no banco de dados.
        /// </summary>
        /// <param name="appointment">Consulta a ser salva.</param>
        Task AddAsync(Appointment appointment);
    }
}
