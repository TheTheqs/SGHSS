// Application/UseCases/Appointments/Read/GetAppointmentLinkUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Appointments;

namespace SGHSS.Application.UseCases.Appointments.GetLink
{
    /// <summary>
    /// Caso de uso responsável por recuperar o link (URL) de teleconsulta
    /// associado a um agendamento específico.
    /// </summary>
    /// <remarks>
    /// Este caso de uso apenas consulta os dados. Toda a lógica de autenticação
    /// e autorização para acesso ao link deve ser tratada na camada de controle
    /// ou middleware da aplicação.
    /// </remarks>
    public class GetAppointmentLinkUseCase
    {
        private readonly IAppointmentRepository _appointmentRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso para consulta de link
        /// de teleconsulta.
        /// </summary>
        /// <param name="appointmentRepository">
        /// Repositório responsável pelo acesso aos dados de agendamentos.
        /// </param>
        public GetAppointmentLinkUseCase(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        /// <summary>
        /// Recupera o link de teleconsulta associado ao agendamento
        /// informado no request.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o identificador do agendamento cuja URL
        /// de teleconsulta deve ser obtida.
        /// </param>
        /// <returns>
        /// Um <see cref="GetAppointmentLinkResponse"/> contendo a URL
        /// da teleconsulta em formato de string.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request fornecido é nulo.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Lançada quando nenhum agendamento é encontrado para o
        /// identificador informado.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o agendamento não possui um link de teleconsulta
        /// configurado.
        /// </exception>
        public async Task<GetAppointmentLinkResponse> Handle(GetAppointmentLinkRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment is null)
            {
                throw new KeyNotFoundException(
                    $"Agendamento com ID {request.AppointmentId} não foi encontrado.");
            }

            if (appointment.Link is null || string.IsNullOrWhiteSpace(appointment.Link.ToString()))
            {
                throw new InvalidOperationException(
                    "O agendamento informado não possui link de teleconsulta configurado.");
            }

            return new GetAppointmentLinkResponse
            {
                Link = appointment.Link.ToString() ?? ""
            };
        }
    }
}
