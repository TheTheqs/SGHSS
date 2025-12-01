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
        /// Atualiza uma consulta existente no banco de dados.
        /// </summary>
        /// <param name="appointment">
        /// Instância de <see cref="Appointment"/> com os dados já modificados
        /// e prontos para serem persistidos.
        /// </param>
        Task UpdateAsync(Appointment appointment);

        /// <summary>
        /// Recupera uma consulta (appointment) pelo seu identificador único.
        /// </summary>
        /// <param name="appointmentId">Identificador da consulta a ser localizada.</param>
        /// <returns>
        /// A entidade <see cref="Appointment"/> correspondente ao ID informado,
        /// ou <c>null</c> caso nenhuma consulta seja encontrada.
        /// </returns>
        Task<Appointment?> GetByIdAsync(Guid appointmentId);

        /// <summary>
        /// Retorna todas as consultas associadas a um paciente específico.
        /// </summary>
        /// <param name="patientId">Identificador do paciente cujas consultas serão recuperadas.</param>
        /// <returns>
        /// Uma lista contendo todas as instâncias de <see cref="Appointment"/>
        /// vinculadas ao paciente informado. Caso o paciente não possua consultas,
        /// a lista retornada será vazia.
        /// </returns>
        Task<List<Appointment>> GetAllByPatientIdAsync(Guid patientId);
    }
}
