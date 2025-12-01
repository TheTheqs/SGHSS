// Interface/Controllers/AppointmentsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Appointments.Read;
using SGHSS.Application.UseCases.Appointments.Register;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas a consultas (appointments),
    /// incluindo agendamento e consulta de histórico de consultas de pacientes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : BaseApiController
    {
        private readonly ScheduleAppointmentUseCase _scheduleAppointmentUseCase;
        private readonly GetPatientAppointmentsUseCase _getPatientAppointmentsUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de consultas.
        /// </summary>
        /// <param name="scheduleAppointmentUseCase">
        /// Caso de uso responsável por agendar novas consultas.
        /// </param>
        /// <param name="getPatientAppointmentsUseCase">
        /// Caso de uso responsável por consultar o histórico de consultas de um paciente.
        /// </param>
        /// <param name="registerLogActivityUseCase">
        /// Caso de uso responsável por registrar logs de atividade.
        /// </param>
        public AppointmentsController(
            ScheduleAppointmentUseCase scheduleAppointmentUseCase,
            GetPatientAppointmentsUseCase getPatientAppointmentsUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _scheduleAppointmentUseCase = scheduleAppointmentUseCase;
            _getPatientAppointmentsUseCase = getPatientAppointmentsUseCase;
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

            // Sem nível de acesso válido → acesso negado
            if (accessLevel is null || accessLevel.Value < AccessLevel.Patient)
                return Forbid();

            // Se for exatamente Patient, só pode consultar o próprio histórico
            if (accessLevel.Value == AccessLevel.Patient)
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
    }
}
