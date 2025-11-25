// Application/UseCases/Patients/Update/UpdateMedicalRecordUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Patients.Update
{
    /// <summary>
    /// Caso de uso responsável por registrar uma nova atualização
    /// no prontuário médico de um paciente.
    /// </summary>
    /// <remarks>
    /// Este caso de uso:
    /// <list type="number">
    /// <item>Valida a existência das entidades envolvidas (paciente, profissional, unidade de saúde e, opcionalmente, consulta);</item>
    /// <item>Localiza o prontuário associado ao paciente;</item>
    /// <item>Cria um <see cref="MedicalRecordUpdate"/> com a descrição clínica e carimbos de data;</item>
    /// <item>Persiste a atualização por meio do agregado do paciente.</item>
    /// </list>
    /// A data da atualização é definida internamente (UTC), garantindo consistência em todo o sistema.
    /// </remarks>
    public sealed class UpdateMedicalRecordUseCase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IProfessionalRepository _professionalRepository;
        private readonly IHealthUnitRepository _healthUnitRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de atualização de prontuário médico.
        /// </summary>
        /// <param name="patientRepository">Repositório de pacientes utilizado para acesso ao prontuário.</param>
        /// <param name="professionalRepository">Repositório de profissionais de saúde.</param>
        /// <param name="healthUnitRepository">Repositório de unidades de saúde.</param>
        /// <param name="appointmentRepository">Repositório de consultas/apontamentos.</param>
        public UpdateMedicalRecordUseCase(
            IPatientRepository patientRepository,
            IProfessionalRepository professionalRepository,
            IHealthUnitRepository healthUnitRepository,
            IAppointmentRepository appointmentRepository)
        {
            _patientRepository = patientRepository;
            _professionalRepository = professionalRepository;
            _healthUnitRepository = healthUnitRepository;
            _appointmentRepository = appointmentRepository;
        }

        /// <summary>
        /// Executa o fluxo de registro de uma nova atualização no prontuário médico.
        /// </summary>
        /// <param name="request">Dados necessários para registrar a atualização.</param>
        /// <returns>
        /// Um <see cref="UpdateMedicalRecordResponse"/> contendo os metadados
        /// da atualização registrada.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando alguma das entidades necessárias não é encontrada
        /// (paciente, prontuário, profissional, unidade, consulta) ou quando
        /// a descrição é inválida.
        /// </exception>
        public async Task<UpdateMedicalRecordResponse> Handle(UpdateMedicalRecordRequest request)
        {
            // Validação básica de descrição
            if (string.IsNullOrWhiteSpace(request.Description))
            {
                throw new InvalidOperationException("A descrição da atualização de prontuário não pode ser vazia.");
            }

            // 1. Carrega o paciente (com seu prontuário)
            var patient = await _patientRepository.GetByIdAsync(request.PatientId);

            if (patient is null)
            {
                throw new InvalidOperationException("Paciente não encontrado para o identificador informado.");
            }

            if (patient.MedicalRecord is null)
            {
                throw new InvalidOperationException("O paciente informado não possui prontuário associado.");
            }

            var medicalRecord = patient.MedicalRecord;

            // 2. Carrega o profissional responsável
            var professional = await _professionalRepository.GetByIdAsync(request.ProfessionalId);

            if (professional is null)
            {
                throw new InvalidOperationException("Profissional não encontrado para o identificador informado.");
            }

            // 3. Carrega a unidade de saúde
            var healthUnit = await _healthUnitRepository.GetByIdAsync(request.HealthUnitId);

            if (healthUnit is null)
            {
                throw new InvalidOperationException("Unidade de saúde não encontrada para o identificador informado.");
            }

            // 4. (Opcional) Carrega a consulta associada, se fornecida
            Appointment? appointment = null;

            if (request.AppointmentId.HasValue)
            {
                appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId.Value);

                if (appointment is null)
                {
                    throw new InvalidOperationException("A consulta informada não foi encontrada.");
                }
            }

            // 5. Cria a nova atualização de prontuário
            var update = new MedicalRecordUpdate
            {
                UpdateDate = DateTime.UtcNow,
                Description = request.Description,
                MedicalRecord = medicalRecord,
                Professional = professional,
                HealthUnit = healthUnit,
                Appointment = appointment
            };

            // 6. Associa a atualização ao prontuário
            medicalRecord.Updates.Add(update);

            // 7. Persiste as alterações via agregado do paciente
            await _patientRepository.UpdateAsync(patient);

            // 8. Monta o response
            return new UpdateMedicalRecordResponse(
                medicalRecordId: medicalRecord.Id,
                medicalRecordUpdateId: update.Id,
                updateDate: update.UpdateDate,
                description: update.Description
            );
        }
    }
}
