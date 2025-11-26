// Application/Interfaces/Repositories/IHomeCareRepository.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define operações de acesso a dados relacionadas a registros
    /// de atendimentos de home care.
    /// </summary>
    public interface IHomeCareRepository
    {
        /// <summary>
        /// Persiste um novo registro de home care no banco de dados.
        /// </summary>
        /// <param name="homeCare">Instância de <see cref="HomeCare"/> a ser salva.</param>
        Task AddAsync(HomeCare homeCare);

        /// <summary>
        /// Atualiza um registro de home care existente no banco de dados.
        /// </summary>
        /// <param name="homeCare">
        /// Entidade <see cref="HomeCare"/> com os dados já modificados e pronta
        /// para ser persistida.
        /// </param>
        Task UpdateAsync(HomeCare homeCare);

        /// <summary>
        /// Recupera um registro de home care pelo seu identificador único.
        /// </summary>
        /// <param name="homeCareId">Identificador do registro de home care a ser localizado.</param>
        /// <returns>
        /// A entidade <see cref="HomeCare"/> correspondente ao ID informado,
        /// ou <c>null</c> caso nenhum registro seja encontrado.
        /// </returns>
        Task<HomeCare?> GetByIdAsync(Guid homeCareId);

        /// <summary>
        /// Recupera todos os registros de home care associados a um paciente.
        /// </summary>
        /// <param name="patientId">Identificador do paciente.</param>
        /// <returns>
        /// Coleção somente leitura contendo os registros de home care
        /// vinculados ao paciente informado.
        /// </returns>
        Task<IReadOnlyCollection<HomeCare>> GetByPatientIdAsync(Guid patientId);
    }
}
