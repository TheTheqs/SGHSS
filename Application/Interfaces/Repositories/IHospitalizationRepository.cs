// Application/Interfaces/Repositories/IHospitalizationRepository.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define as operações de persistência relacionadas às internações (Hospitalizations)
    /// no sistema. Esta interface abstrai o acesso ao repositório responsável
    /// por recuperar dados de internações armazenadas no banco de dados.
    /// </summary>
    /// <remarks>
    /// Implementações dessa interface devem garantir que a recuperação
    /// seja realizada de maneira eficiente e consistente, retornando
    /// dados completos quando necessário para regras de negócio,
    /// como alta de pacientes ou auditorias.
    /// </remarks>
    public interface IHospitalizationRepository
    {
        /// <summary>
        /// Recupera uma internação existente pelo seu identificador exclusivo.
        /// </summary>
        /// <param name="hospitalizationId">
        /// O identificador único da internação a ser buscada.
        /// </param>
        /// <returns>
        /// Uma tarefa assíncrona contendo:
        /// <list type="bullet">
        /// <item>A instância <see cref="Hospitalization"/> correspondente ao ID informado;</item>
        /// <item><c>null</c> caso nenhuma internação seja encontrada.</item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// A responsabilidade de validar a existência da internação e
        /// lidar com o retorno nulo pertence à camada de aplicação.
        /// </remarks>
        Task<Hospitalization?> GetByIdAsync(Guid hospitalizationId);

        /// <summary>
        /// Adiciona uma nova internação ao repositório.
        /// </summary>
        /// <param name="hospitalization">
        /// A entidade <see cref="Hospitalization"/> a ser persistida.
        /// </param>
        /// <returns>
        /// Uma tarefa assíncrona que representa a operação de inclusão.
        /// </returns>
        /// <remarks>
        /// A validação de regras de negócio e consistência deve ser realizada
        /// previamente pela camada de aplicação.
        /// </remarks>
        Task AddAsync(Hospitalization hospitalization);
    }
}
