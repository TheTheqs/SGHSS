// Interface/Controllers/AppointmentsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Appointments.Register;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas a consultas (appointments),
    /// incluindo o agendamento de novas consultas a partir de slots de agenda disponíveis.
    /// </summary>
    /// <remarks>
    /// As operações deste controlador exigem autenticação e, no caso de agendamento,
    /// são restritas a usuários com nível de acesso exatamente
    /// <see cref="AccessLevel.Patient"/>.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : BaseApiController
    {
        private readonly ScheduleAppointmentUseCase _scheduleAppointmentUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de consultas.
        /// </summary>
        /// <param name="scheduleAppointmentUseCase">
        /// Caso de uso responsável por agendar novas consultas.
        /// </param>
        /// <param name="registerLogActivityUseCase">
        /// Caso de uso responsável por registrar logs de atividade.
        /// </param>
        public AppointmentsController(
            ScheduleAppointmentUseCase scheduleAppointmentUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _scheduleAppointmentUseCase = scheduleAppointmentUseCase;
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
            // Apenas usuários com nível EXATAMENTE Patient
            if (!HasExactAccessLevel(AccessLevel.Patient))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta agendada com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _scheduleAppointmentUseCase.Handle(request);

                // 201 Created, pois um novo recurso (Appointment) foi criado
                return CreatedAtAction(nameof(Schedule), response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao agendar consulta: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao agendar consulta: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao agendar consulta.";
                throw;
            }
            finally
            {
                // Ainda não temos HealthUnitId direto no request/appointment aqui,
                // então deixamos como null até existir essa informação na borda da aplicação.
                await RegistrarLogAsync(
                    userId,
                    action: "Appointments.Schedule",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
