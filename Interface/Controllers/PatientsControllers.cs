// Interface/Controllers/PatientsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Administrators.Update;
using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Application.UseCases.Patients.ConsultMedicalRecord;
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
        private readonly HospitalizePatientUseCase _hospitalizePatientUseCase;
        private readonly DischargePatientUseCase _dischargePatientUseCase;
        private readonly ConsultMedicalRecordUseCase _consultMedicalRecordUseCase;

        /// <summary>
        /// Instancia um novo controlador de pacientes,
        /// habilitando registro, listagem de pacientes e gerenciamento de hospitalização.
        /// </summary>
        public PatientsController(
            RegisterPatientUseCase registerPatientUseCase,
            GetAllPatientsUseCase getAllPatientsUseCase,
            HospitalizePatientUseCase hospitalizePatientUseCase,
            DischargePatientUseCase dischargePatientUseCase,
            ConsultMedicalRecordUseCase consultMedicalRecordUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _registerPatientUseCase = registerPatientUseCase;
            _getAllPatientsUseCase = getAllPatientsUseCase;
            _hospitalizePatientUseCase = hospitalizePatientUseCase;
            _dischargePatientUseCase = dischargePatientUseCase;
            _consultMedicalRecordUseCase = consultMedicalRecordUseCase;
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

        /// <summary>
        /// Hospitaliza um paciente vinculando-o a uma cama disponível.
        /// Apenas profissionais (AccessLevel.Professional) podem executar esta ação.
        /// </summary>
        /// <param name="request">Informações necessárias para hospitalizar o paciente.</param>
        /// <returns>Um <see cref="HospitalizePatientResponse"/> contendo detalhes da internação.</returns>
        [HttpPost("hospitalize")]
        [Authorize]
        [ProducesResponseType(typeof(HospitalizePatientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<HospitalizePatientResponse>> Hospitalize(
            [FromBody] HospitalizePatientRequest request)
        {
            // 🔒 Regra: EXATAMENTE AccessLevel.Professional
            var accessLevelClaim = User.FindFirst("access_level")?.Value;

            if (!Enum.TryParse(accessLevelClaim, out AccessLevel userAccessLevel) ||
                userAccessLevel != AccessLevel.Professional)
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Paciente hospitalizado com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _hospitalizePatientUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao hospitalizar paciente: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao hospitalizar paciente: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao hospitalizar paciente.";
                throw;
            }
            finally
            {
                // Dá pra usar request.BedId depois quando quiser registrar a unidade
                await RegistrarLogAsync(
                    userId,
                    action: "Patients.Hospitalize",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Realiza a alta de um paciente, encerrando a internação ativa
        /// e liberando o leito associado.
        /// </summary>
        /// <param name="patientId">Identificador do paciente que receberá alta.</param>
        [HttpPost("{patientId:guid}/discharge")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Discharge(Guid patientId)
        {
            // 🔒 Regra: EXATAMENTE AccessLevel.Professional
            var accessLevelClaim = User.FindFirst("access_level")?.Value;

            if (!Enum.TryParse(accessLevelClaim, out AccessLevel userAccessLevel) ||
                userAccessLevel != AccessLevel.Professional)
            {
                return Forbid();
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Alta de paciente realizada com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                await _dischargePatientUseCase.Handle(patientId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao realizar alta: {ex.Message}";

                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao realizar alta: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao realizar alta.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Patients.Discharge",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }

        /// <summary>
        /// Consulta o prontuário eletrônico de um paciente específico.
        /// </summary>
        /// <param name="patientId">
        /// Identificador do paciente cujo prontuário será consultado.
        /// </param>
        /// <returns>
        /// Um <see cref="ConsultMedicalRecordResponse"/> contendo o prontuário
        /// e o histórico de atualizações.
        /// </returns>
        [HttpGet("{patientId:guid}/medical-record")]
        [Authorize]
        [ProducesResponseType(typeof(ConsultMedicalRecordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ConsultMedicalRecordResponse>> ConsultMedicalRecord(Guid patientId)
        {
            var userId = GetUserId();
            var accessLevel = GetUserAccessLevel();

            // Sem nível de acesso válido
            if (accessLevel is null || accessLevel.Value < AccessLevel.Patient)
                return Forbid();

            // Se for exatamente Patient, só pode consultar o próprio prontuário
            if (HasExactAccessLevel(AccessLevel.Patient))
            {
                if (!userId.HasValue || userId.Value != patientId)
                {
                    return Forbid();
                }
            }

            LogResult logResult = LogResult.Success;
            string logDescription = "Consulta de prontuário realizada com sucesso.";

            try
            {
                var request = new ConsultMedicalRecordRequest
                {
                    PatientId = patientId
                };

                var response = await _consultMedicalRecordUseCase.Handle(request);
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar prontuário: {ex.Message}";

                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                // Paciente não encontrado OU sem prontuário associado.
                logResult = LogResult.Failure;
                logDescription = $"Falha ao consultar prontuário: {ex.Message}";

                return NotFound(new
                {
                    error = ex.Message
                });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao consultar prontuário.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "Patients.ConsultMedicalRecord",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: null
                );
            }
        }
    }
}
