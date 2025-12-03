// Interface/Controllers/AppointmentsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Appointments;
using SGHSS.Application.UseCases.Appointments.GetLink;
using SGHSS.Application.UseCases.Appointments.Read;
using SGHSS.Application.UseCases.Appointments.Register;
using SGHSS.Application.UseCases.Appointments.Update;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas a consultas (appointments),
    /// incluindo agendamento, consulta de histórico e acesso a links de teleconsulta.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : BaseApiController
    {
        private readonly ScheduleAppointmentUseCase _scheduleAppointmentUseCase;
        private readonly GetPatientAppointmentsUseCase _getPatientAppointmentsUseCase;
        private readonly GetAppointmentLinkUseCase _getAppointmentLinkUseCase;
        private readonly CompleteAppointmentUseCase _completeAppointmentUseCase;
        private readonly UpdateAppointmentStatusUseCase _updateAppointmentStatusUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de consultas.
        /// </summary>
        /// <param name="scheduleAppointmentUseCase">
        /// Caso de uso responsável por agendar novas consultas.
        /// </param>
        /// <param name="getPatientAppointmentsUseCase">
        /// Caso de uso responsável por consultar o histórico de consultas de um paciente.
        /// </param>
        /// <param name="getAppointmentLinkUseCase">
        /// Caso de uso responsável por consultar o link de teleconsulta de uma consulta específica.
        /// </param>
        /// <param name="registerLogActivityUseCase">
        /// Caso de uso responsável por registrar logs de atividade.
        /// </param>
        /// <param name="completeAppointmentUseCase">
        /// Caso de uso responsável por finalizar uma consulta.
        /// </param>
        public AppointmentsController(
            ScheduleAppointmentUseCase scheduleAppointmentUseCase,
            GetPatientAppointmentsUseCase getPatientAppointmentsUseCase,
            GetAppointmentLinkUseCase getAppointmentLinkUseCase,
            CompleteAppointmentUseCase completeAppointmentUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase,
            UpdateAppointmentStatusUseCase updateAppointmentStatusUseCase)
            : base(registerLogActivityUseCase)
        {
            _scheduleAppointmentUseCase = scheduleAppointmentUseCase;
            _getPatientAppointmentsUseCase = getPatientAppointmentsUseCase;
            _getAppointmentLinkUseCase = getAppointmentLinkUseCase;
            _completeAppointmentUseCase = completeAppointmentUseCase;
            _updateAppointmentStatusUseCase = updateAppointmentStatusUseCase;
        }

        /// <summary>
        /// Realiza o agendamento de uma nova consulta para um paciente,
        /// utilizando um slot de agenda disponível de um profissional.
        /// </summary>
        /// <remarks>
        /// Este endpoint:
        /// <list type="bullet">
        /// <item>Verifica conflitos com outros agendamentos do profissional;</item>
        /// <item>Garante que o horário esteja dentro da política de agendamento (SchedulePolicy);</item>
        /// <item>Cria o <c>ScheduleSlot</c> correspondente e um <c>Appointment</c> associado;</item>
        /// <item>Gera um link de teleconsulta (quando aplicável) e retorna os identificadores principais.</item>
        /// </list>
        ///
        /// Apenas usuários autenticados com nível de acesso exatamente
        /// <see cref="AccessLevel.Patient"/> podem agendar consultas.
        /// </remarks>
        /// <param name="request">Dados necessários para realizar o agendamento da consulta.</param>
        /// <returns>
        /// Um <see cref="ScheduleAppointmentResponse"/> contendo o identificador da consulta,
        /// o identificador do slot de agenda criado e, quando aplicável, o link de teleconsulta.
        /// </returns>
        [HttpPost("schedule")]
        [Authorize]
        [ProducesResponseType(typeof(ScheduleAppointmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ScheduleAppointmentResponse>> Schedule(
            [FromBody] ScheduleAppointmentRequest request)
        {
            // Apenas usuários com nível EXATAMENTE Patient podem agendar
            if (!HasExactAccessLevel(AccessLevel.Patient))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta agendada com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _scheduleAppointmentUseCase.Handle(request);

                return CreatedAtAction(nameof(Schedule), response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao agendar consulta: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao agendar consulta: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao agendar consulta.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Appointments.Schedule",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Retorna o histórico de consultas (appointments) associadas a um paciente específico.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Qualquer usuário com nível de acesso <see cref="AccessLevel.Patient"/> ou superior pode chamar o endpoint;</item>
        /// <item>Se o usuário tiver nível de acesso exatamente <see cref="AccessLevel.Patient"/>,
        /// ele só poderá consultar o histórico do próprio paciente (ID do paciente deve ser igual ao ID do usuário no token);</item>
        /// <item>Usuários com nível superior (por exemplo, Professional, Basic, Super) podem consultar o histórico de qualquer paciente.</item>
        /// </list>
        /// </remarks>
        /// <param name="patientId">Identificador do paciente cujas consultas serão listadas.</param>
        /// <returns>
        /// Um <see cref="GetPatientAppointmentsResponse"/> contendo o identificador do paciente
        /// e a lista de suas consultas em formato resumido.
        /// </returns>
        [HttpGet("patient/{patientId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(GetPatientAppointmentsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<GetPatientAppointmentsResponse>> GetPatientAppointments(
            [FromRoute] Guid patientId)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Sem nível de acesso válido
            if (accessLevel is null || accessLevel.Value < AccessLevel.Patient)
                return Forbid();

            // Se for exatamente Patient, só pode consultar o próprio histórico
            if (HasExactAccessLevel(AccessLevel.Patient))
            {
                if (!userId.HasValue || userId.Value != patientId)
                {
                    return Forbid();
                }
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de histórico de consultas do paciente realizada com sucesso.";

            var request = new GetPatientAppointmentsRequest
            {
                PatientId = patientId
            };

            try
            {
                var response = await _getPatientAppointmentsUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar histórico de consultas do paciente: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar histórico de consultas do paciente: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar histórico de consultas do paciente.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Appointments.GetByPatient",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Retorna o link (URL) de teleconsulta associado a um agendamento específico.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Qualquer usuário autenticado com nível de acesso <see cref="AccessLevel.Patient"/> ou superior pode acessar;</item>
        /// <item>A validação fina de permissão (se o usuário pode ou não ver o link daquele agendamento)
        /// deve ser complementada via políticas adicionais ou em middleware, conforme necessidade.</item>
        /// </list>
        /// </remarks>
        /// <param name="appointmentId">Identificador do agendamento cuja URL de teleconsulta será recuperada.</param>
        /// <returns>
        /// Um <see cref="GetAppointmentLinkResponse"/> contendo a URL da teleconsulta em formato de string.
        /// </returns>
        [HttpGet("{appointmentId:guid}/link")]
        [Authorize]
        [ProducesResponseType(typeof(GetAppointmentLinkResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetAppointmentLinkResponse>> GetAppointmentLink(
            [FromRoute] Guid appointmentId)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Sem nível de acesso válido → acesso negado
            if (accessLevel is null || accessLevel.Value < AccessLevel.Patient)
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Link de teleconsulta recuperado com sucesso.";

            var request = new GetAppointmentLinkRequest
            {
                AppointmentId = appointmentId
            };

            try
            {
                var response = await _getAppointmentLinkUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao recuperar link de teleconsulta: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao recuperar link de teleconsulta: {ex.Message}";

                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao recuperar link de teleconsulta: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao recuperar link de teleconsulta.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Appointments.GetTeleconsultationLink",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Conclui uma consulta previamente agendada, permitindo a emissão opcional
        /// de documentos clínicos associados (atestado digital, prescrição eletrônica
        /// e atualização de prontuário).
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Somente usuários autenticados com nível de acesso
        /// <see cref="AccessLevel.Professional"/> ou superior podem concluir consultas;</item>
        /// </list>
        /// O fluxo assume que a consulta já esteja em status
        /// <see cref="AppointmentStatus.Confirmed"/> e que o agregado
        /// <c>Appointment</c> possa ser carregado a partir do identificador
        /// informado no <see cref="CompleteAppointmentRequest.AppointmentId"/>.
        /// </remarks>
        /// <param name="request">
        /// Dados necessários para identificar a consulta e, opcionalmente,
        /// emitir documentos clínicos vinculados a ela.
        /// </param>
        /// <returns>
        /// Um <see cref="CompleteAppointmentResponse"/> contendo o status final da consulta
        /// e os identificadores dos registros clínicos gerados (quando houver).
        /// </returns>
        [HttpPut("complete")]
        [Authorize]
        [ProducesResponseType(typeof(CompleteAppointmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CompleteAppointmentResponse>> Complete(
            [FromBody] CompleteAppointmentRequest request)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            if (accessLevel is null || !HasMinimumAccessLevel(AccessLevel.Professional))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta concluída com sucesso.";

            try
            {
                var response = await _completeAppointmentUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao concluir consulta: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao concluir consulta: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao concluir consulta.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Appointments.Complete",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Atualiza o status de uma consulta (appointment) e do slot de agenda
        /// associado, permitindo operações como cancelamento, ausência do paciente,
        /// recusa de atendimento, entre outras transições válidas.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Somente usuários autenticados com nível de acesso
        /// <see cref="AccessLevel.Professional"/> ou superior podem atualizar
        /// o status de consultas;</item>
        /// </list>
        /// A validação das transições permitidas entre
        /// <see cref="AppointmentStatus"/> e <see cref="ScheduleSlotStatus"/>
        /// é responsabilidade do caso de uso <see cref="UpdateAppointmentStatusUseCase"/>.
        /// </remarks>
        /// <param name="request">
        /// Dados necessários para identificar a consulta e aplicar os novos status
        /// à consulta e ao slot de agenda associado.
        /// </param>
        /// <returns>
        /// Um <see cref="UpdateAppointmentStatusResponse"/> contendo os status finais
        /// da consulta e do slot de agenda após a atualização.
        /// </returns>
        [HttpPut("status")]
        [Authorize]
        [ProducesResponseType(typeof(UpdateAppointmentStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UpdateAppointmentStatusResponse>> UpdateStatus(
            [FromBody] UpdateAppointmentStatusRequest request)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Somente Professional ou superior pode alterar status de consulta
            if (accessLevel is null || !HasMinimumAccessLevel(AccessLevel.Professional))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Status da consulta atualizado com sucesso.";

            try
            {
                var response = await _updateAppointmentStatusUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao atualizar status da consulta: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao atualizar status da consulta: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao atualizar status da consulta.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Appointments.UpdateStatus",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
