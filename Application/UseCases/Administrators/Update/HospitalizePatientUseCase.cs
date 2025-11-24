// Application/UseCases/Administrators/Update/HospitalizePatientUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Administrators.Update
{
    /// <summary>
    /// Caso de uso responsável por hospitalizar um paciente, criando um registro de internação
    /// e associando-o a uma cama disponível.
    /// </summary>
    /// <remarks>
    /// Este caso de uso:
    /// <list type="bullet">
    /// <item>Garante que a cama informada exista e esteja disponível;</item>
    /// <item>Garante que o paciente não possua outra internação ativa (alta não registrada);</item>
    /// <item>Cria uma nova entidade <see cref="Hospitalization"/> com status <see cref="HospitalizationStatus.Admitted"/>;</item>
    /// <item>Atualiza o status da cama para <see cref="BedStatus.Occupied"/>.</item>
    /// </list>
    /// A atualização de prontuário (MedicalRecord) será adicionada posteriormente,
    /// vinculando a internação a um histórico clínico mais abrangente.
    /// </remarks>
    public sealed class HospitalizePatientUseCase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IBedRepository _bedRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de hospitalização de paciente.
        /// </summary>
        /// <param name="patientRepository">Repositório de pacientes utilizado para carregar e persistir o agregado.</param>
        /// <param name="bedRepository">Repositório de camas utilizado para recuperar e atualizar o leito associado.</param>
        public HospitalizePatientUseCase(
            IPatientRepository patientRepository,
            IBedRepository bedRepository)
        {
            _patientRepository = patientRepository;
            _bedRepository = bedRepository;
        }

        /// <summary>
        /// Executa o fluxo de hospitalização de um paciente, criando uma internação ativa
        /// e ocupando a cama informada.
        /// </summary>
        /// <param name="request">Dados necessários para a internação do paciente.</param>
        /// <returns>
        /// Um <see cref="HospitalizePatientResponse"/> contendo informações essenciais
        /// sobre a internação criada.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Lançada quando o paciente ou a cama não são encontrados.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a cama não está disponível ou o paciente já possui uma internação ativa.
        /// </exception>
        public async Task<HospitalizePatientResponse> Handle(HospitalizePatientRequest request)
        {
            // Carrega o paciente, incluindo suas internações (configuração de Include deve ser feita no repositório).
            Patient? patient = await _patientRepository.GetByIdAsync(request.PatientId);

            if (patient is null)
                throw new KeyNotFoundException("Nenhum paciente foi encontrado com o ID informado.");

            // Carrega a cama a ser utilizada.
            Bed? bed = await _bedRepository.GetByIdAsync(request.BedId);

            if (bed is null)
                throw new KeyNotFoundException("Nenhuma cama foi encontrada com o ID informado.");

            // Verifica se a cama está disponível para internação.
            if (bed.Status != BedStatus.Available)
                throw new InvalidOperationException("A cama informada não está disponível para internação.");

            // Verifica se o paciente já possui alguma internação ativa (sem alta registrada).
            bool hasActiveHospitalization = patient.Hospitalizations
                .Any(h => h.DischargeDate is null);

            if (hasActiveHospitalization)
                throw new InvalidOperationException("O paciente já possui uma internação ativa.");

            // Cria a entidade de internação.
            var hospitalization = new Hospitalization
            {
                AdmissionDate = DateTimeOffset.UtcNow,
                DischargeDate = null, // explicitamente sem alta
                Reason = request.Reason,
                Status = HospitalizationStatus.Admitted,
                Bed = bed
            };

            // Associa a internação ao paciente.
            patient.Hospitalizations.Add(hospitalization);

            // Atualiza o status da cama para ocupada.
            bed.Status = BedStatus.Occupied;

            // Persistência das mudanças.
            await _patientRepository.UpdateAsync(patient);
            await _bedRepository.UpdateAsync(bed);

            // TODO: Registrar atualização no prontuário (MedicalRecordUpdate)
            // associada a esta internação, vinculando o evento ao histórico clínico.

            return new HospitalizePatientResponse(
                hospitalizationId: hospitalization.Id,
                patientId: patient.Id,
                bedId: bed.Id,
                admissionDate: hospitalization.AdmissionDate,
                reason: hospitalization.Reason
            );
        }
    }
}
