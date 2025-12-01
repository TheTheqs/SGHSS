// Application/Interfaces/Repositories/IDigitalMedicalCertificateRepository.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    public interface IDigitalMedicalCertificateRepository
    {
        /// <summary>
        /// Adiciona um novo atestado ao repositório.
        /// </summary>
        /// <param name="certificate">
        /// Instância de <see cref="DigitalMedicalCertificate"/> que será persistida.
        /// </param>
        /// <returns>
        /// Uma tarefa que representa a operação assíncrona.
        /// </returns>
        Task AddAsync(DigitalMedicalCertificate certificate);

        /// <summary>
        /// Retorna todos os atestados médicos digitais associados ao paciente informado.
        /// </summary>
        /// <param name="patientId">Identificador do paciente.</param>
        /// <returns>
        /// Uma lista contendo os atestados emitidos para o paciente,
        /// ou uma lista vazia caso nenhum registro seja encontrado.
        /// </returns>
        Task<IReadOnlyList<DigitalMedicalCertificate>> GetByPatientIdAsync(Guid patientId);
    }
}
