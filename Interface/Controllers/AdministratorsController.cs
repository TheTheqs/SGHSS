// Interface/Controllers/AdministratorsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Administrators.Read;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas a administradores do sistema,
    /// incluindo o registro de novos administradores e a consulta de administradores já cadastrados.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AdministratorsController : BaseApiController
    {
        private readonly RegisterAdministratorUseCase _registerAdministratorUseCase;
        private readonly GetAllAdministratorsUseCase _getAllAdministratorsUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador.
        /// </summary>
        /// <param name="registerAdministratorUseCase">Caso de uso responsável por registrar administradores.</param>
        /// <param name="registerLogActivityUseCase">Caso de uso responsável por registrar logs de atividade.</param>
        public AdministratorsController(
            RegisterAdministratorUseCase registerAdministratorUseCase,
            GetAllAdministratorsUseCase getAllAdministratorsUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _registerAdministratorUseCase = registerAdministratorUseCase;
            _getAllAdministratorsUseCase = getAllAdministratorsUseCase;
        }

        /// <summary>
        /// Registra um novo Administrador no sistema.
        /// Somente Administradores Super podem acessar este endpoint.
        /// </summary>
        /// <param name="request">Dados necessários para criar o Administrador.</param>
        /// <returns>Um <see cref="RegisterAdministratorResponse"/> contendo o ID do Administrador criado.</returns>
        [HttpPost("register")]
        [Authorize]
        [ProducesResponseType(typeof(RegisterAdministratorResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegisterAdministratorResponse>> Register(
            [FromBody] RegisterAdministratorRequest request)
        {
            // Apenas Administrador Super pode criar novos Administradores
            if (!HasMinimumAccessLevel(AccessLevel.Super))
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Administrador criado com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _registerAdministratorUseCase.Handle(request);
                return CreatedAtAction(nameof(Register), response);
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar administrador: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar administrador: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao registrar administrador.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Administrators.Register",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Retorna a lista de todos os administradores cadastrados no sistema,
        /// em formato resumido (ID e Nome).
        /// Requer um Administrador autenticado com nível Manager (3) ou superior.
        /// </summary>
        /// <returns>
        /// Um <see cref="GetAllResponse"/> contendo a coleção de administradores.
        /// </returns>
        [HttpGet("all")]
        [Authorize]
        [ProducesResponseType(typeof(GetAllResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GetAllResponse>> GetAll()
        {
            // Apenas Administradores com nível Manager (3) ou superior
            if (!HasMinimumAccessLevel(AccessLevel.Manager))
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de administradores realizada com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _getAllAdministratorsUseCase.Handle();
                return Ok(response);
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar administradores.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Administrators.GetAll",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
