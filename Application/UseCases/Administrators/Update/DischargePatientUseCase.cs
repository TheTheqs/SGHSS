// Application/UseCases/Administrators/Update/DischargePatientUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Administrators.Update
{
    /// <summary>
    /// Caso de uso responsável por realizar a alta de um paciente,
    /// encerrando a internação ativa e liberando o leito associado.
    /// </summary>
    /// <remarks>
    /// Este caso de uso:
    /// <list type="bullet">
    /// <item>Garante que o paciente exista e seja carregado com suas internações;</item>
    /// <item>Localiza a internação ativa (sem data de alta registrada);</item>
    /// <item>Define a data de alta como o instante UTC atual;</item>
    /// <item>Atualiza o status da internação para <see cref="HospitalizationStatus.Discharged"/>;</item>
    /// <item>Atualiza o status da cama para <see cref="BedStatus.Available"/>.</item>
    /// </list>
    /// Pressupõe-se que exista no máximo uma internação ativa por paciente.
    /// </remarks>
    public sealed class DischargePatientUseCase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IBedRepository _bedRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de alta de paciente.
        /// </summary>
        /// <param name="patientRepository">Repositório de pacientes utilizado para carregar e persistir o agregado.</param>
        /// <param name="bedRepository">Repositório de camas utilizado para recuperar e atualizar o leito associado.</param>
        public DischargePatientUseCase(
            IPatientRepository patientRepository,
            IBedRepository bedRepository)
        {
            _patientRepository = patientRepository;
            _bedRepository = bedRepository;
        }

        /// <summary>
        /// Executa o fluxo de alta de um paciente, encerrando a internação ativa
        /// e liberando a cama correspondente.
        /// </summary>
        /// <param name="patientId">Identificador do paciente que receberá alta.</param>
        /// <exception cref="KeyNotFoundException">
        /// Lançada quando o paciente não é encontrado.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o paciente não possui internação ativa ou quando a internação está em estado inconsistente.
        /// </exception>
        public async Task Handle(Guid patientId)
        {
            // Carrega o paciente e suas internações.
            Patient? patient = await _patientRepository.GetByIdAsync(patientId);

            if (patient is null)
                throw new KeyNotFoundException("Nenhum paciente foi encontrado com o ID informado.");

            // Identifica a internação ativa (sem alta).
            Hospitalization? activeHospitalization = patient.Hospitalizations
                .FirstOrDefault(h => h.DischargeDate is null);

            if (activeHospitalization is null)
                throw new InvalidOperationException("O paciente não possui uma internação ativa.");

            // Verifica se a internação está corretamente associada a um leito.
            if (activeHospitalization.Bed is null)
                throw new InvalidOperationException("A internação ativa não está associada a nenhuma cama.");

            Bed bed = activeHospitalization.Bed;

            // Garante que o leito está realmente ocupado.
            if (bed.Status != BedStatus.Occupied)
                throw new InvalidOperationException("A cama associada à internação ativa não está ocupada.");

            // Define a alta.
            activeHospitalization.DischargeDate = DateTimeOffset.UtcNow;
            activeHospitalization.Status = HospitalizationStatus.Discharged;

            // Libera o leito.
            bed.Status = BedStatus.Available;

            // Persistência.
            await _patientRepository.UpdateAsync(patient);
            await _bedRepository.UpdateAsync(bed);
        }
    }
}
