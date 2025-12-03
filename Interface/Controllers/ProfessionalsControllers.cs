// Interface/Controllers/ProfessionalsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Application.UseCases.Professionals.Read;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Application.UseCases.Professionals.Update;
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
        private readonly UpdateProfessionalSchedulePolicyUseCase _updateProfessionalSchedulePolicyUseCase;
        private readonly ConsultReservedScheduleSlotsUseCase _consultReservedScheduleSlotsUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de profissionais.
        /// </summary>
        public ProfessionalsController(
            RegisterProfessionalUseCase registerProfessionalUseCase,
            GetAllProfessionalsUseCase getAllProfessionalsUseCase,
            GenerateAvailableSlotsUseCase generateAvailableSlotsUseCase,
            UpdateProfessionalSchedulePolicyUseCase updateProfessionalSchedulePolicyUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase,
            ConsultReservedScheduleSlotsUseCase consultReservedScheduleSlotsUseCase)
            : base(registerLogActivityUseCase)
        {
            _registerProfessionalUseCase = registerProfessionalUseCase;
            _getAllProfessionalsUseCase = getAllProfessionalsUseCase;
            _generateAvailableSlotsUseCase = generateAvailableSlotsUseCase;
            _updateProfessionalSchedulePolicyUseCase = updateProfessionalSchedulePolicyUseCase;
            _consultReservedScheduleSlotsUseCase = consultReservedScheduleSlotsUseCase;
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

        /// <summary>
        /// Atualiza a política de agendamento associada a um profissional.
        /// </summary>
        /// <remarks>
        /// Apenas o próprio profissional (AccessLevel.Professional) pode alterar
        /// a sua própria política de agenda.
        /// </remarks>
        /// <param name="professionalId">
        /// Identificador do profissional cuja política de agenda será atualizada.
        /// </param>
        /// <param name="request">
        /// Dados contendo o identificador do profissional e a nova política de agenda.
        /// </param>
        /// <returns>
        /// Um <see cref="UpdateProfessionalSchedulePolicyResponse"/> contendo o identificador
        /// do profissional e a política de agenda atualmente configurada.
        /// </returns>
        [HttpPut("{professionalId:guid}/schedule-policy")]
        [Authorize]
        [ProducesResponseType(typeof(UpdateProfessionalSchedulePolicyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UpdateProfessionalSchedulePolicyResponse>> UpdateSchedulePolicy(
            Guid professionalId,
            [FromBody] UpdateProfessionalSchedulePolicyRequest request)
        {
            // Somente para profissionais
            var accessLevelClaim = User.FindFirst("access_level")?.Value;

            if (!Enum.TryParse(accessLevelClaim, out AccessLevel userAccessLevel) ||
                userAccessLevel != AccessLevel.Professional)
            {
                return Forbid();
            }

            // Verifica se o ID do token é o mesmo do profissional da rota
            var userId = GetUserId();
            if (!userId.HasValue || userId.Value != professionalId)
            {
                return Forbid();
            }

            // Consistência entre rota e corpo da requisição
            if (request is null || request.ProfessionalId == Guid.Empty || request.ProfessionalId != professionalId)
            {
                return BadRequest(new
                {
                    error = "O identificador do profissional na rota não corresponde ao informado no corpo da requisição."
                });
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Política de agenda do profissional atualizada com sucesso.";

            try
            {
                var response = await _updateProfessionalSchedulePolicyUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao atualizar política de agenda: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao atualizar política de agenda: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao atualizar política de agenda: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao atualizar política de agenda.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Professionals.UpdateSchedulePolicy",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Consulta todos os horários reservados de uma agenda profissional específica.
        /// </summary>
        /// <remarks>
        /// Apenas o próprio profissional (AccessLevel.Professional) pode consultar
        /// os horários reservados de sua agenda.
        /// </remarks>
        /// <param name="professionalId">
        /// Identificador do profissional dono da agenda consultada.
        /// </param>
        /// <param name="request">
        /// Dados contendo o identificador da agenda profissional.
        /// </param>
        /// <returns>
        /// Um <see cref="ConsultReservedScheduleSlotResponse"/> contendo o identificador
        /// da agenda e a coleção de horários reservados.
        /// </returns>
        [HttpPost("{professionalId:guid}/schedules/reserved")]
        [Authorize]
        [ProducesResponseType(typeof(ConsultReservedScheduleSlotResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ConsultReservedScheduleSlotResponse>> ConsultReservedScheduleSlots(
            Guid professionalId,
            [FromBody] ConsultReservedScheduleSlotsRequest request)
        {
            var userId = GetUserId();
            var accessLevel = GetUserAccessLevel();

            // 🔒 Apenas profissionais autenticados
            if (accessLevel is null || accessLevel.Value != AccessLevel.Professional)
            {
                return Forbid();
            }

            // 🔒 Apenas o próprio dono da agenda
            if (!userId.HasValue || userId.Value != professionalId)
            {
                return Forbid();
            }

            // 🔒 Validação básica de consistência do corpo
            if (request is null || request.ProfessionalScheduleId == Guid.Empty)
            {
                return BadRequest(new
                {
                    error = "O identificador da agenda profissional não pode ser vazio."
                });
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de horários reservados realizada com sucesso.";

            try
            {
                var response = await _consultReservedScheduleSlotsUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentNullException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar horários reservados: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar horários reservados: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar horários reservados: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar horários reservados.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Professionals.ConsultReservedScheduleSlots",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

    }
}
