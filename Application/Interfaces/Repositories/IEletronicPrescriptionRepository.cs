// Application/Interfaces/Repositories/IEletronicPrescriptionRepository.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define o contrato para operações de persistência de
    /// <see cref="EletronicPrescription"/> no repositório.
    /// </summary>
    public interface IEletronicPrescriptionRepository
    {
        /// <summary>
        /// Adiciona uma nova prescrição eletrônica ao repositório.
        /// </summary>
        /// <param name="prescription">
        /// Instância de <see cref="EletronicPrescription"/> que será persistida.
        /// </param>
        /// <returns>
        /// Uma tarefa que representa a operação assíncrona.
        /// </returns>
        Task AddAsync(EletronicPrescription prescription);
    }
}
