// Application/Interfaces/Repositories/IHealthUnitRepository.cs

using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define o contrato para operações de persistência relacionadas a unidades de saúde.
    /// </summary>
    /// <remarks>
    /// Esta interface expõe apenas os métodos necessários para a
    /// camada de Application interagir com a infraestrutura de dados,
    /// garantindo isolamento e respeitando os princípios da Clean Architecture.
    /// </remarks>
    public interface IHealthUnitRepository
    {
        /// <summary>
        /// Verifica se já existe uma unidade cadastrada com o CNPJ informado.
        /// </summary>
        /// <param name="cnpj">O CNPJ encapsulado pelo Value Object.</param>
        /// <returns>True se o CNPJ já estiver registrado; caso contrário, False.</returns>
        Task<bool> ExistsByCnpjAsync(Cnpj cnpj);

        /// <summary>
        /// Persiste uma nova unidade de saúde no repositório.
        /// </summary>
        /// <param name="healthUnit">A entidade de unidade de saúde a ser salva.</param>
        Task AddAsync(HealthUnit healthUnit);

        /// <summary>
        /// Recupera uma unidade de saúde pelo seu identificador único.
        /// </summary>
        /// <param name="healthUnitId">O identificador da unidade de saúde.</param>
        /// <returns>
        /// A entidade <see cref="HealthUnit"/> correspondente ao identificador,
        /// ou <c>null</c> caso nenhuma unidade seja encontrada.
        /// </returns>
        Task<HealthUnit?> GetByIdAsync(Guid healthUnitId);

        /// <summary>
        /// Atualiza uma unidade de saúde existente no repositório,
        /// persistindo alterações em sua lista de leitos ou demais propriedades.
        /// </summary>
        /// <param name="healthUnit">A instância atualizada da entidade a ser persistida.</param>
        Task UpdateAsync(HealthUnit healthUnit);




    }
}
