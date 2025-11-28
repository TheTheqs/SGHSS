// Interface/Controllers/BaseApiController.cs

using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador base com utilitários comuns para os demais controllers da API.
    /// </summary>
    public abstract class BaseApiController : ControllerBase
    {
        private readonly RegisterLogActivityUseCase _registerLogActivityUseCase;

        protected BaseApiController(RegisterLogActivityUseCase registerLogActivityUseCase)
        {
            _registerLogActivityUseCase = registerLogActivityUseCase;
        }

        /// <summary>
        /// Registra uma atividade de log associada à requisição atual.
        /// </summary>
        protected async Task RegistrarLogAsync(
            Guid? userId,
            string action,
            string description,
            LogResult result,
            Guid? healthUnitId = null)
        {
            var ipString = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            var ipAddress = new IpAddress(ipString);

            var request = new RegisterLogActivityRequest
            {
                UserId = userId,
                HealthUnitId = healthUnitId,
                Action = action,
                Description = description,
                IpAddress = ipAddress,
                Result = result
            };

            await _registerLogActivityUseCase.Handle(request);
        }

        /// <summary>
        /// Verifica se o usuário autenticado possui ao menos o nível de acesso informado.
        /// </summary>
        protected bool HasMinimumAccessLevel(AccessLevel minimumLevel)
        {
            var claim = User?.FindFirst("access_level")?.Value;
            if (string.IsNullOrWhiteSpace(claim))
            {
                return false;
            }

            if (Enum.TryParse<AccessLevel>(claim, out var parsedLevel))
            {
                return parsedLevel >= minimumLevel;
            }

            if (int.TryParse(claim, out var numeric) &&
                numeric >= (int)minimumLevel)
            {
                return true;
            }

            return false;
        }
    }
}
