// Application/UseCases/Appointments/Read/GetPatientAppointmentsResponse.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Appointments.Read
{
    /// <summary>
    /// Representa o resultado da consulta de todas as consultas (appointments)
    /// associadas a um paciente.
    /// </summary>
    /// <remarks>
    /// Este response expõe o identificador do paciente e a lista de consultas
    /// em formato de <see cref="AppointmentDto"/>, permitindo exibição em telas
    /// de prontuário, histórico ou relatórios.
    /// </remarks>
    public sealed class GetPatientAppointmentsResponse
    {
        /// <summary>
        /// Identificador do paciente ao qual as consultas pertencem.
        /// </summary>
        public Guid PatientId { get; }

        /// <summary>
        /// Coleção de consultas associadas ao paciente.
        /// </summary>
        public IReadOnlyCollection<AppointmentDto> Appointments { get; }

        /// <summary>
        /// Cria uma nova instância do response de consultas do paciente.
        /// </summary>
        /// <param name="patientId">Identificador do paciente.</param>
        /// <param name="appointments">Lista de consultas associadas ao paciente.</param>
        public GetPatientAppointmentsResponse(
            Guid patientId,
            IReadOnlyCollection<AppointmentDto> appointments)
        {
            PatientId = patientId;
            Appointments = appointments;
        }
    }
}
