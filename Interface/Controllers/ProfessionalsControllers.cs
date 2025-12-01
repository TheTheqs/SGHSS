// Interface/Controllers/ProfessionalsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Application.UseCases.Professionals.Read;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Application.UseCases.ProfessionalSchedules.Consult;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas a profissionais,
    /// incluindo registro, consulta geral e verificação de disponibilidade de agenda.
    /// Requer permissões administrativas com nível Patient (0) ou superior.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProfessionalsController : BaseApiController
    {
        private readonly RegisterProfessionalUseCase _registerProfessionalUseCase;
        private readonly GetAllProfessionalsUseCase _getAllProfessionalsUseCase;
        private readonly GenerateAvailableSlotsUseCase _generateAvailableSlotsUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de profissionais.
        /// </summary>
        public ProfessionalsController(
            RegisterProfessionalUseCase registerProfessionalUseCase,
            GetAllProfessionalsUseCase getAllProfessionalsUseCase,
            GenerateAvailableSlotsUseCase generateAvailableSlotsUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _registerProfessionalUseCase = registerProfessionalUseCase;
            _getAllProfessionalsUseCase = getAllProfessionalsUseCase;
            _generateAvailableSlotsUseCase = generateAvailableSlotsUseCase;
        }

        /// <summary>
        /// Registra um novo profissional no sistema.
        /// Requer um Administrador autenticado com nível de acesso básico ou superior.
        /// </summary>
        /// <param name="request">Dados necessários para criar o profissional.</param>
        /// <returns>
        /// Um <see cref="RegisterProfessionalResponse"/> contendo o ID do profissional criado.
        /// </returns>
        [HttpPost("register")]
        [Authorize]
        [ProducesResponseType(typeof(RegisterProfessionalResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegisterProfessionalResponse>> Register(
            [FromBody] RegisterProfessionalRequest request)
        {
            // Apenas Administradores com nível Basic (2) ou superior
            if (!HasMinimumAccessLevel(AccessLevel.Basic))
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Profissional criado com sucesso.";
            Guid? userId = GetUserId(); // admin que está registrando o profissional

            try
            {
                var response = await _registerProfessionalUseCase.Handle(request);
                return CreatedAtAction(nameof(Register), response);
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar profissional: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar profissional: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao registrar profissional.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Professionals.Register",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Retorna uma lista resumida contendo todos os profissionais cadastrados,
        /// exibindo apenas ID e Nome. Disponível para Administradores Patient (0) ou superior.
        /// </summary>
        [HttpGet("all")]
        [Authorize]
        [ProducesResponseType(typeof(GetAllResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GetAllResponse>> GetAll()
        {
            // Apenas Administradores Patient (0) ou superior
            if (!HasMinimumAccessLevel(AccessLevel.Patient))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de todos os profissionais realizada com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var result = await _getAllProfessionalsUseCase.Handle();
                return Ok(result);
            }
            catch (Exception ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar profissionais: {ex.Message}";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Professionals.GetAll",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null);
            }
        }

        /// <summary>
        /// Consulta e retorna os horários disponíveis na agenda de um profissional
        /// em um determinado intervalo de tempo.
        /// </summary>
        /// <remarks>
        /// Quando <see cref="GenerateAvailableSlotsRequest.From"/> não é informado,
        /// o sistema utiliza a data/hora atual como início. Quando
        /// <see cref="GenerateAvailableSlotsRequest.To"/> não é informada,
        /// o sistema utiliza um horizonte padrão de dois meses a partir da data inicial.
        /// 
        /// Disponível para Administradores com nível Patient (0) ou superior.
        /// </remarks>
        /// <param name="request">
        /// Dados necessários para identificar o profissional e o intervalo de consulta.
        /// </param>
        /// <returns>
        /// Um <see cref="GenerateAvailableSlotsResponse"/> contendo o identificador do profissional
        /// e a lista de intervalos de horários disponíveis para agendamento.
        /// </returns>
        [HttpPost("schedules/available")]
        [Authorize]
        [ProducesResponseType(typeof(GenerateAvailableSlotsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GenerateAvailableSlotsResponse>> GenerateAvailableSlots(
            [FromBody] GenerateAvailableSlotsRequest request)
        {
            // Apenas Administradores Patient (0) ou superior
            if (!HasMinimumAccessLevel(AccessLevel.Patient))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de horários disponíveis realizada com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _generateAvailableSlotsUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar horários disponíveis: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar horários disponíveis: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar horários disponíveis.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Professionals.GenerateAvailableSlots",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null);
            }
        }
    }
}
