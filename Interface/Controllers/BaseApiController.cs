// Interface/Controllers/BaseApiController.cs

using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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

        // ==============================================
        // LOG ACTIVITY
        // ==============================================

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

        // ==============================================
        // CLAIM HELPERS
        // ==============================================

        /// <summary>
        /// Obtém o ID do usuário logado (claim 'sub').
        /// </summary>
        protected Guid? GetUserId()
        {
            var value =
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (value == null)
                return null;

            return Guid.TryParse(value, out var parsed)
                ? parsed
                : null;
        }

        /// <summary>
        /// Obtém o e-mail do usuário logado.
        /// </summary>
        protected string? GetUserEmail()
        {
            return
                User.FindFirstValue(JwtRegisteredClaimNames.Email) ??
                User.FindFirstValue(ClaimTypes.Email);
        }

        /// <summary>
        /// Obtém o tipo concreto do usuário (Administrator, Professional, Patient, etc.).
        /// </summary>
        protected string? GetUserType()
        {
            return User.FindFirstValue("user_type");
        }

        /// <summary>
        /// Obtém o nível de acesso do usuário como enum AccessLevel.
        /// </summary>
        protected AccessLevel? GetUserAccessLevel()
        {
            var claim = User.FindFirstValue("access_level");
            if (string.IsNullOrWhiteSpace(claim))
                return null;

            // Permite tanto número quanto nome do enum
            if (Enum.TryParse<AccessLevel>(claim, out var parsedLevel))
                return parsedLevel;

            if (int.TryParse(claim, out var numeric))
                return (AccessLevel)numeric;

            return null;
        }

        /// <summary>
        /// Retorna todas as claims do usuário autenticado (para debug / auditoria).
        /// </summary>
        protected IReadOnlyDictionary<string, string> GetAllClaims()
        {
            return User.Claims.ToDictionary(c => c.Type, c => c.Value);
        }

        // ==============================================
        // AUTHORIZATION HELPERS
        // ==============================================

        /// <summary>
        /// Verifica se o usuário possui ao menos o nível de acesso informado.
        /// </summary>
        protected bool HasMinimumAccessLevel(AccessLevel minimumLevel)
        {
            var access = GetUserAccessLevel();
            if (access is null)
                return false;

            return access.Value >= minimumLevel;
        }
    }
}
