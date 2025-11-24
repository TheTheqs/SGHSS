// Application/Interfaces/Repositories/IPatientRepository.cs

using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define o contrato de acesso a dados para operações relacionadas à entidade <see cref="Patient"/>.
    /// </summary>
    /// <remarks>
    /// Esta interface é utilizada pela camada de Application para orquestrar casos de uso envolvendo pacientes,
    /// sem depender diretamente da tecnologia de persistência (por exemplo, Entity Framework Core).
    /// Implementações concretas devem residir na camada de Infra.
    /// </remarks>
    public interface IPatientRepository
    {
        /// <summary>
        /// Verifica se já existe um paciente cadastrado com o CPF informado.
        /// </summary>
        /// <param name="cpf">CPF em formato textual (normalizado pela camada de aplicação).</param>
        /// <returns>
        /// <c>true</c> se já existir um paciente com o CPF informado; caso contrário, <c>false</c>.
        /// </returns>
        Task<bool> ExistsByCpfAsync(Cpf cpf);

        /// <summary>
        /// Recupera um paciente seu identificador único,
        /// incluindo suas listas e relacionamentos relevantes
        /// </summary>
        /// <param name="patientId">Identificador do paciente.</param>
        /// <returns>
        /// A entidade <see cref="Patient"/> correspondente ao identificador,
        /// ou <c>null</c> caso não seja encontrada.
        /// </returns>
        /// <remarks>
        /// Inclui o carregamento explícito de seus relacionamentos.
        /// </remarks>
        Task<Patient?> GetByIdAsync(Guid patientId);

        /// <summary>
        /// Determina de forma assíncrona se existe um usuário com o endereço de e-mail especificado.
        /// </summary>
        /// <param name="email">O endereço de e-mail a ser verificado. Não pode ser nulo.</param>
        /// <returns>Uma tarefa que representa a operação assíncrona. O resultado da tarefa contém
        /// <see langword="true"/> se existir um usuário com o e-mail especificado; caso contrário,
        /// <see langword="false"/>.</returns>
        Task<bool> ExistsByEmailAsync(Email email);

        /// <summary>
        /// Atualiza o registro de um paciente na base de dados, incluindo suas
        /// propriedades diretas e quaisquer entidades agregadas, como internações
        /// (<see cref="Hospitalization"/>) e prontuário médico.
        /// </summary>
        /// <param name="patient">
        /// Instância atualizada da entidade <see cref="Patient"/> que será persistida.
        /// Esta instância deve refletir o estado atual desejado do paciente e de seus
        /// relacionamentos carregados.
        /// </param>
        /// <returns>
        /// Uma tarefa assíncrona que representa a operação de atualização.
        /// </returns>
        /// <remarks>
        /// Este método utiliza o rastreamento de alterações do Entity Framework Core.
        /// Caso o paciente ou seus agregados já estejam sendo rastreados pelo contexto,
        /// o EF detectará automaticamente as modificações e aplicará as mudanças durante
        /// a chamada ao método de persistência.
        /// </remarks>
        Task UpdateAsync(Patient patient);

        /// <summary>
        /// Adiciona um novo paciente ao repositório.
        /// </summary>
        /// <param name="patient">Instância de <see cref="Patient"/> a ser persistida.</param>
        Task AddAsync(Patient patient);
    }
}
