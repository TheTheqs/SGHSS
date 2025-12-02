// Interface/Controllers/NotificationsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Application.UseCases.Notifications.Read;
using SGHSS.Application.UseCases.Notifications.Update;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas a notificações do sistema,
    /// incluindo consulta das notificações do usuário e atualização de status.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : BaseApiController
    {
        private readonly GetUserNotificationsUseCase _getUserNotificationsUseCase;
        private readonly UpdateNotificationStatusUseCase _updateNotificationStatusUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de notificações.
        /// </summary>
        /// <param name="getUserNotificationsUseCase">
        /// Caso de uso responsável por consultar notificações de um usuário.
        /// </param>
        /// <param name="updateNotificationStatusUseCase">
        /// Caso de uso responsável por atualizar o status de uma notificação.
        /// </param>
        /// <param name="registerLogActivityUseCase">
        /// Caso de uso responsável por registrar logs de atividade.
        /// </param>
        public NotificationsController(
            GetUserNotificationsUseCase getUserNotificationsUseCase,
            UpdateNotificationStatusUseCase updateNotificationStatusUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _getUserNotificationsUseCase = getUserNotificationsUseCase;
            _updateNotificationStatusUseCase = updateNotificationStatusUseCase;
        }

        /// <summary>
        /// Retorna as notificações associadas ao usuário autenticado.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Somente usuários autenticados com nível de acesso
        /// <see cref="AccessLevel.Patient"/> ou superior podem consultar
        /// suas próprias notificações;</item>
        /// </list>
        /// O identificador do usuário é obtido a partir do token JWT.
        /// </remarks>
        /// <returns>
        /// Um <see cref="GetUserNotificationsResponse"/> contendo a lista de
        /// notificações do usuário autenticado.
        /// </returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(GetUserNotificationsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GetUserNotificationsResponse>> GetMyNotifications()
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Mínimo: Patient
            if (accessLevel is null || !HasMinimumAccessLevel(AccessLevel.Patient))
                return Forbid();

            if (!userId.HasValue)
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de notificações do usuário autenticado realizada com sucesso.";

            var request = new GetUserNotificationsRequest
            {
                UserId = userId.Value
            };

            try
            {
                var response = await _getUserNotificationsUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar notificações do usuário: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar notificações do usuário: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar notificações do usuário.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Notifications.GetMyNotifications",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Atualiza o status de uma notificação específica.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Somente usuários autenticados com nível de acesso
        /// <see cref="AccessLevel.Super"/> podem atualizar o status de notificações
        /// de forma arbitrária;</item>
        /// </list>
        /// Este endpoint é voltado para uso administrativo.
        /// </remarks>
        /// <param name="request">
        /// Dados necessários para identificar a notificação e definir o novo status.
        /// </param>
        /// <returns>
        /// Um <see cref="UpdateNotificationStatusResponse"/> contendo o estado
        /// final da notificação após a atualização.
        /// </returns>
        [HttpPut("status")]
        [Authorize]
        [ProducesResponseType(typeof(UpdateNotificationStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UpdateNotificationStatusResponse>> UpdateStatus(
            [FromBody] UpdateNotificationStatusRequest request)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Apenas Super pode atualizar status de notificações genericamente
            if (accessLevel is null || !HasMinimumAccessLevel(AccessLevel.Super))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Status de notificação atualizado com sucesso.";

            try
            {
                var response = await _updateNotificationStatusUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao atualizar status de notificação: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao atualizar status de notificação: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao atualizar status de notificação: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao atualizar status de notificação.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Notifications.UpdateStatus",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
