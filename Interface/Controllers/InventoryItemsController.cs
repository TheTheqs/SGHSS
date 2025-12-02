// Interface/Controllers/InventoryItemsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.InventoryItems.Consult;
using SGHSS.Application.UseCases.InventoryItems.Register;
using SGHSS.Application.UseCases.InventoryItems.Update;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas a itens de estoque
    /// (inventory items) de unidades de saúde, incluindo cadastro, consulta
    /// e movimentações de inventário.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryItemsController : BaseApiController
    {
        private readonly RegisterInventoryItemUseCase _registerInventoryItemUseCase;
        private readonly ConsultInventoryItemUseCase _consultInventoryItemUseCase;
        private readonly RegisterInventoryMovementUseCase _registerInventoryMovementUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de itens de inventário.
        /// </summary>
        /// <param name="registerInventoryItemUseCase">
        /// Caso de uso responsável por registrar novos itens de estoque.
        /// </param>
        /// <param name="consultInventoryItemUseCase">
        /// Caso de uso responsável por consultar itens de estoque.
        /// </param>
        /// <param name="registerInventoryMovementUseCase">
        /// Caso de uso responsável por registrar movimentações de estoque.
        /// </param>
        /// <param name="registerLogActivityUseCase">
        /// Caso de uso responsável pelo registro de logs de atividade.
        /// </param>
        public InventoryItemsController(
            RegisterInventoryItemUseCase registerInventoryItemUseCase,
            ConsultInventoryItemUseCase consultInventoryItemUseCase,
            RegisterInventoryMovementUseCase registerInventoryMovementUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _registerInventoryItemUseCase = registerInventoryItemUseCase;
            _consultInventoryItemUseCase = consultInventoryItemUseCase;
            _registerInventoryMovementUseCase = registerInventoryMovementUseCase;
        }

        /// <summary>
        /// Registra um novo item de estoque vinculado a uma unidade de saúde.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Somente usuários autenticados com nível de acesso
        /// <see cref="AccessLevel.Basic"/> ou superior podem cadastrar
        /// itens de estoque;</item>
        /// </list>
        /// O caso de uso valida a unidade de saúde, a unidade de medida
        /// e as regras básicas de negócio antes de persistir o item.
        /// </remarks>
        /// <param name="request">
        /// Dados necessários para criação do item de inventário.
        /// </param>
        /// <returns>
        /// Um <see cref="RegisterInventoryItemResponse"/> contendo os dados
        /// consolidados do item recém-cadastrado.
        /// </returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(RegisterInventoryItemResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegisterInventoryItemResponse>> Register(
            [FromBody] RegisterInventoryItemRequest request)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Nível mínimo para operar estoque: Basic
            if (accessLevel is null || !HasMinimumAccessLevel(AccessLevel.Basic))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Item de estoque registrado com sucesso.";

            try
            {
                var response = await _registerInventoryItemUseCase.Handle(request);

                return CreatedAtAction(
                    nameof(Register),
                    new { id = response.Id },
                    response
                );
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar item de estoque: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar item de estoque: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao registrar item de estoque.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "InventoryItems.Register",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: request.HealthUnitId
                );
            }
        }

        /// <summary>
        /// Registra uma movimentação de estoque (entrada, saída ou ajuste)
        /// para um item de inventário, atualizando a quantidade disponível
        /// e registrando o histórico da operação.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Somente usuários autenticados com nível de acesso
        /// <see cref="AccessLevel.Basic"/> ou superior podem registrar
        /// movimentações de estoque;</item>
        /// </list>
        /// A consistência de estoque, validação de identificadores e
        /// vínculo com administrador responsável são garantidos pelo
        /// <see cref="RegisterInventoryMovementUseCase"/>.
        /// </remarks>
        /// <param name="request">
        /// Dados necessários para registrar a movimentação de estoque,
        /// incluindo item, unidade, quantidade, tipo e administrador.
        /// </param>
        /// <returns>
        /// Um <see cref="RegisterInventoryMovementResponse"/> contendo o
        /// estado atualizado do item de estoque após a movimentação.
        /// </returns>
        [HttpPost("movement")]
        [Authorize]
        [ProducesResponseType(typeof(RegisterInventoryMovementResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegisterInventoryMovementResponse>> RegisterMovement(
            [FromBody] RegisterInventoryMovementRequest request)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Nível mínimo para operar estoque: Basic
            if (accessLevel is null || !HasMinimumAccessLevel(AccessLevel.Basic))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Movimentação de estoque registrada com sucesso.";

            try
            {
                var response = await _registerInventoryMovementUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar movimentação de estoque: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar movimentação de estoque: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao registrar movimentação de estoque.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "InventoryItems.RegisterMovement",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: request.HealthUnitId
                );
            }
        }

        /// <summary>
        /// Consulta os dados de um item de estoque vinculado a uma unidade de saúde,
        /// retornando informações como nome, unidade de medida e quantidade atual em estoque.
        /// </summary>
        /// <remarks>
        /// Regra de autorização:
        /// <list type="bullet">
        /// <item>Somente usuários autenticados com nível de acesso
        /// <see cref="AccessLevel.Basic"/> ou superior podem consultar
        /// itens de estoque;</item>
        /// </list>
        /// O caso de uso garante que o item retornado pertença à unidade de saúde
        /// informada, garantindo a consistência dos dados.
        /// </remarks>
        /// <param name="healthUnitId">
        /// Identificador da unidade de saúde à qual o item deve estar associado.
        /// </param>
        /// <param name="inventoryItemId">
        /// Identificador do item de estoque a ser consultado.
        /// </param>
        /// <returns>
        /// Um <see cref="ConsultInventoryItemResponse"/> contendo as informações
        /// consolidadas do item de estoque consultado.
        /// </returns>
        [HttpGet("{healthUnitId:guid}/{inventoryItemId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ConsultInventoryItemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ConsultInventoryItemResponse>> Consult(
            [FromRoute] Guid healthUnitId,
            [FromRoute] Guid inventoryItemId)
        {
            var accessLevel = GetUserAccessLevel();
            var userId = GetUserId();

            // Nível mínimo para operar/consultar estoque: Basic
            if (accessLevel is null || !HasMinimumAccessLevel(AccessLevel.Basic))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de item de estoque realizada com sucesso.";

            var request = new ConsultInventoryItemRequest
            {
                HealthUnitId = healthUnitId,
                InventoryItemId = inventoryItemId
            };

            try
            {
                var response = await _consultInventoryItemUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar item de estoque: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar item de estoque: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar item de estoque.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "InventoryItems.Consult",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: healthUnitId
                );
            }
        }
    }
}
