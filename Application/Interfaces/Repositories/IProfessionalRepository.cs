// Application/Interfaces/Repositories/IProfessionalRepository.cs

using SGHSS.Domain.ValueObjects;
using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define o contrato de acesso a dados para operações relacionadas à entidade
    /// <see cref="Professional"/>.
    /// </summary>
    /// <remarks>
    /// Esta interface é utilizada pela camada de Application para orquestrar casos de uso que envolvem
    /// profissionais da saúde, garantindo que a aplicação permaneça desacoplada da tecnologia de persistência.
    /// Implementações concretas devem residir na camada de Infra.
    /// </remarks>
    public interface IProfessionalRepository
    {
        /// <summary>
        /// Verifica de forma assíncrona se já existe um profissional cadastrado com o registro profissional informado.
        /// </summary>
        /// <param name="license">
        /// O registro profissional encapsulado em um <see cref="ProfessionalLicense"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> se existir um profissional com o registro informado; caso contrário, <c>false</c>.
        /// </returns>
        Task<bool> ExistsByProfessionalLicenseAsync(ProfessionalLicense license);

        /// <summary>
        /// Determina de forma assíncrona se já existe um profissional cadastrado com o e-mail especificado.
        /// </summary>
        /// <param name="email">
        /// O endereço de e-mail encapsulado em um <see cref="Email"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> se existir um profissional com o e-mail informado; caso contrário, <c>false</c>.
        /// </returns>
        Task<bool> ExistsByEmailAsync(Email email);

        /// <summary>
        /// Adiciona um novo profissional ao repositório.
        /// </summary>
        /// <param name="professional">
        /// Instância de <see cref="Professional"/> que será persistida.
        /// </param>
        /// <returns>
        /// Uma tarefa que representa a operação assíncrona.
        /// </returns>
        Task AddAsync(Professional professional);

        /// <summary>
        /// Recupera um profissional pelo seu identificador único.
        /// </summary>
        /// <param name="professionalId">Identificador do profissional a ser localizado.</param>
        /// <returns>
        /// A entidade <see cref="Professional"/> correspondente ao ID informado,
        /// ou <c>null</c> caso nenhum profissional seja encontrado.
        /// </returns>
        Task<Professional?> GetByIdAsync(Guid professionalId);

        /// <summary>
        /// Atualiza o registro de um profissional na base de dados, incluindo suas
        /// propriedades diretas e quaisquer entidades agregadas, como agenda e política.
        /// </summary>
        /// <param name="professional">
        /// Instância atualizada da entidade <see cref="Professional"/> que será persistida.
        /// Esta instância deve refletir o estado atual desejado do profissional e de seus
        /// relacionamentos carregados.
        /// </param>
        /// <returns>
        /// Uma tarefa assíncrona que representa a operação de atualização.
        /// </returns>
        /// <remarks>
        /// Este método utiliza o rastreamento de alterações do Entity Framework Core.
        /// Caso o profissional ou seus agregados já estejam sendo rastreados pelo contexto,
        /// o EF detectará automaticamente as modificações e aplicará as mudanças durante
        /// a chamada ao método de persistência.
        /// </remarks>
        Task UpdateAsync(Professional professional);
    }
}
