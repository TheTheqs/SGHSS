// Application/UseCases/Appointments/Read/GetAppointmentLinkRequest.cs

namespace SGHSS.Application.UseCases.Appointments.GetLink
{
    /// <summary>
    /// Representa a solicitação para consultar o link de teleconsulta
    /// associado a um agendamento específico.
    /// </summary>
    /// <remarks>
    /// Este request contém apenas o identificador do agendamento,
    /// pois toda a validação de permissões e credenciais ocorre na
    /// camada de controle ou middleware.
    /// </remarks>
    public sealed class GetAppointmentLinkRequest
    {
        /// <summary>
        /// Identificador único do agendamento cuja URL de teleconsulta
        /// deve ser recuperada.
        /// </summary>
        public Guid AppointmentId { get; init; }
    }
}
