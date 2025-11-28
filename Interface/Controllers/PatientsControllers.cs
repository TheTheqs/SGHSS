// Interface/Controllers/PatientsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Application.UseCases.Patients.Read;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por registrar novos pacientes no sistema.
    /// Apenas Administradores com nível de acesso básico ou superior
    /// possuem permissão para acessar este endpoint.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : BaseApiController
    {
        private readonly RegisterPatientUseCase _registerPatientUseCase;
        private readonly GetAllPatientsUseCase _getAllPatientsUseCase;

        /// <summary>
        /// Instancia um novo controlador de pacientes,
        /// habilitando registro e listagem de pacientes.
        /// </summary>
        public PatientsController(
            RegisterPatientUseCase registerPatientUseCase,
            GetAllPatientsUseCase getAllPatientsUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _registerPatientUseCase = registerPatientUseCase;
            _getAllPatientsUseCase = getAllPatientsUseCase;
        }

        /// <summary>
        /// Registra um novo paciente no sistema.
        /// Requer um Administrador autenticado com nível de acesso básico ou superior.
        /// </summary>
        /// <param name="request">Dados necessários para criar o paciente.</param>
        /// <returns>
        /// Um <see cref="RegisterPatientResponse"/> contendo o ID do paciente criado.
        /// </returns>
        [HttpPost("register")]
        [Authorize]
        [ProducesResponseType(typeof(RegisterPatientResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<RegisterPatientResponse>> Register(
            [FromBody] RegisterPatientRequest request)
        {
            // Apenas Administradores com nível Basic (2) ou superior
            if (!HasMinimumAccessLevel(AccessLevel.Basic))
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Paciente criado com sucesso.";
            Guid? userId = GetUserId(); // admin que está registrando o paciente

            try
            {
                var response = await _registerPatientUseCase.Handle(request);
                return CreatedAtAction(nameof(Register), response);
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar paciente: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao registrar paciente: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao registrar paciente.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Patients.Register",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Obtém todos os pacientes do sistema em formato resumido (ID + Nome).
        /// Requer nível de acesso Basic (2) ou superior.
        /// </summary>
        [HttpGet("all")]
        [Authorize]
        [ProducesResponseType(typeof(GetAllResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GetAllResponse>> GetAll()
        {
            if (!HasMinimumAccessLevel(AccessLevel.Basic))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de todos os pacientes realizada com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _getAllPatientsUseCase.Handle();
                return Ok(response);
            }
            catch (Exception ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar pacientes: {ex.Message}";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Patients.GetAll",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
