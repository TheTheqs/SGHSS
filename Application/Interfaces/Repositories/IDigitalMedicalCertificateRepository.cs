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
    }
}
