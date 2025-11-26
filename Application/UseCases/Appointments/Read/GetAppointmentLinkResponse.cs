// Application/UseCases/Appointments/Read/GetAppointmentLinkResponse.cs

namespace SGHSS.Application.UseCases.Appointments
{
    /// <summary>
    /// Representa a resposta contendo o link (URL) da teleconsulta
    /// associado ao agendamento solicitado.
    /// </summary>
    /// <remarks>
    /// O valor retornado é sempre uma string primitiva,
    /// extraída de <see cref="Domain.ValueObjects.Link.Value"/>,
    /// para facilitar a saída via API.
    /// </remarks>
    public sealed class GetAppointmentLinkResponse
    {
        /// <summary>
        /// URL da teleconsulta associada ao agendamento.
        /// </summary>
        public string Link { get; init; } = string.Empty;
    }
}
