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
    }
}
