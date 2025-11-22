// Application/Interfaces/Repositories/IHealthUnitRepository.cs

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
        Task AddAsync(Domain.Models.HealthUnit healthUnit);
    }
}
