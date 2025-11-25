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

        /// <summary>
        /// Recupera uma consulta (appointment) pelo seu identificador único.
        /// </summary>
        /// <param name="appointmentId">Identificador da consulta a ser localizada.</param>
        /// <returns>
        /// A entidade <see cref="Appointment"/> correspondente ao ID informado,
        /// ou <c>null</c> caso nenhuma consulta seja encontrada.
        /// </returns>
        Task<Appointment?> GetByIdAsync(Guid appointmentId);
    }
}
