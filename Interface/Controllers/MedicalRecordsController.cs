// Interface/Controllers/MedicalRecordsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGHSS.Application.UseCases.Patients.Update;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Domain.Enums;

namespace SGHSS.Interface.Controllers
{
    /// <summary>
    /// Controlador responsável por operações relacionadas ao prontuário médico dos pacientes,
    /// incluindo o registro de novas atualizações clínicas no prontuário.
    /// </summary>
    /// <remarks>
    /// Todas as operações deste controlador exigem autenticação e são restritas
    /// a usuários com nível de acesso exatamente <see cref="AccessLevel.Professional"/>.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalRecordsController : BaseApiController
    {
        private readonly UpdateMedicalRecordUseCase _updateMedicalRecordUseCase;

        /// <summary>
        /// Cria uma nova instância do controlador de prontuários médicos.
        /// </summary>
        public MedicalRecordsController(
            UpdateMedicalRecordUseCase updateMedicalRecordUseCase,
            RegisterLogActivityUseCase registerLogActivityUseCase)
            : base(registerLogActivityUseCase)
        {
            _updateMedicalRecordUseCase = updateMedicalRecordUseCase;
        }

        /// <summary>
        /// Registra uma nova atualização clínica no prontuário médico de um paciente.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite que um profissional registre anotações clínicas,
        /// associando opcionalmente a atualização a uma consulta específica.
        /// Apenas usuários autenticados com nível de acesso exatamente
        /// <see cref="AccessLevel.Professional"/> podem realizar esta operação.
        /// </remarks>
        /// <param name="request">Dados necessários para registrar a atualização de prontuário.</param>
        /// <returns>
        /// Um <see cref="UpdateMedicalRecordResponse"/> contendo os metadados da atualização.
        /// </returns>
        [HttpPost("update")]
        [Authorize]
        [ProducesResponseType(typeof(UpdateMedicalRecordResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UpdateMedicalRecordResponse>> UpdateMedicalRecord(
            [FromBody] UpdateMedicalRecordRequest request)
        {
            // Garante nível de acesso exato PROFESSIONAL
            if (!HasExactAccessLevel(AccessLevel.Professional))
                return Forbid();

            LogResult logResult = LogResult.Success;
            string logDescription = "Atualização de prontuário registrada com sucesso.";
            Guid? userId = GetUserId();

            try
            {
                var response = await _updateMedicalRecordUseCase.Handle(request);
                return CreatedAtAction(nameof(UpdateMedicalRecord), response);
            }
            catch (InvalidOperationException ex)
            {
                logResult = LogResult.Failure;
                logDescription = $"Falha ao atualizar prontuário: {ex.Message}";

                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                logResult = LogResult.Failure;
                logDescription = "Erro inesperado ao registrar atualização de prontuário.";
                throw;
            }
            finally
            {
                await RegistrarLogAsync(
                    userId,
                    action: "MedicalRecords.Update",
                    description: logDescription,
                    result: logResult,
                    healthUnitId: request.HealthUnitId
                );
            }
        }
    }
}
