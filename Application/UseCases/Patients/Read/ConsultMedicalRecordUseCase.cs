// Application/UseCases/Patients/Read/ConsultMedicalRecord/ConsultMedicalRecordUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Patients.ConsultMedicalRecord
{
    /// <summary>
    /// Caso de uso responsável por consultar o prontuário médico de um paciente
    /// a partir do seu identificador.
    /// </summary>
    /// <remarks>
    /// Este caso de uso encapsula a lógica de busca do paciente e de seu prontuário,
    /// realizando as validações necessárias e projetando as entidades de domínio para
    /// DTOs adequados à exposição pela camada de interface.
    /// </remarks>
    public class ConsultMedicalRecordUseCase
    {
        private readonly IPatientRepository _patientRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de consulta de prontuário.
        /// </summary>
        /// <param name="patientRepository">
        /// Repositório responsável por acessar os dados de pacientes na camada de infraestrutura.
        /// </param>
        public ConsultMedicalRecordUseCase(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        /// <summary>
        /// Executa o fluxo de consulta de prontuário para o paciente informado.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o identificador do paciente cujo prontuário será consultado.
        /// </param>
        /// <returns>
        /// Um <see cref="ConsultMedicalRecordResponse"/> contendo o identificador do paciente
        /// e os dados do seu prontuário, incluindo o histórico de atualizações.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request é nulo.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Lançada quando o identificador do paciente é vazio.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o paciente não é encontrado ou quando não há prontuário associado.
        /// </exception>
        public async Task<ConsultMedicalRecordResponse> Handle(ConsultMedicalRecordRequest request)
        {
            // Validação defensiva básica de entrada
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request), "A requisição não pode ser nula.");
            }

            if (request.PatientId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O identificador do paciente não pode ser vazio.",
                    nameof(request.PatientId)
                );
            }

            // Busca o paciente no repositório.
            var patient = await _patientRepository.GetByIdAsync(request.PatientId);

            if (patient is null)
            {
                // Paciente não encontrado na base.
                throw new InvalidOperationException(
                    "Não foi possível localizar um paciente para o identificador informado."
                );
            }

            if (patient.MedicalRecord is null)
            {
                // Paciente existente, porém sem prontuário associado.
                // Regra de negócio: tratar como operação inválida.
                throw new InvalidOperationException(
                    "O paciente não possui um prontuário associado."
                );
            }

            var medicalRecord = patient.MedicalRecord;

            // Projeção do prontuário para o DTO exposto pela aplicação.
            var medicalRecordDto = new MedicalRecordDto
            {
                Id = medicalRecord.Id,
                // ToString() é utilizado para converter o Value Object de número de prontuário
                // em uma representação textual apropriada para a camada de interface.
                Number = medicalRecord.Number.ToString(),
                CreatedAt = medicalRecord.CreatedAt,
                Updates = medicalRecord.Updates
                    // Ordena da atualização mais recente para a mais antiga,
                    // facilitando a exibição em interfaces usuais de timeline.
                    .OrderByDescending(update => update.UpdateDate)
                    .Select(update => new MedicalRecordUpdateDto
                    {
                        Id = update.Id,
                        UpdateDate = update.UpdateDate,
                        Description = update.Description,
                        ProfessionalId = update.Professional.Id,
                        HealthUnitId = update.HealthUnit.Id,
                        AppointmentId = update.Appointment?.Id
                    })
                    .ToList()
            };

            // Monta o response final
            return new ConsultMedicalRecordResponse
            {
                PatientId = patient.Id,
                MedicalRecord = medicalRecordDto
            };
        }
    }
}
