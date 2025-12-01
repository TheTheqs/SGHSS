// Application/UseCases/Appointments/Read/GetPatientAppointmentsRequest.cs

namespace SGHSS.Application.UseCases.Appointments.Read
{
    /// <summary>
    /// Representa os dados necessários para consultar todas as consultas
    /// (appointments) associadas a um paciente específico.
    /// </summary>
    public sealed class GetPatientAppointmentsRequest
    {
        /// <summary>
        /// Identificador do paciente cujas consultas serão listadas.
        /// </summary>
        public Guid PatientId { get; init; }
    }
}
