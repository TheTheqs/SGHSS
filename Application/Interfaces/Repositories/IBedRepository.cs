// Application/Interfaces/Repositories/IBedRepository.cs

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define as operações de persistência relacionadas às camas (Beds)
    /// no sistema. Esta interface abstrai o acesso ao repositório responsável
    /// por atualizar informações de uma cama no banco de dados.
    /// </summary>
    /// <remarks>
    /// Implementações dessa interface devem garantir que a atualização seja
    /// realizada de maneira consistente e reflita corretamente as mudanças 
    /// de estado da entidade, como disponibilidade, manutenção ou ocupação.
    /// </remarks>
    public interface IBedRepository
    {
        /// <summary>
        /// Atualiza os dados de uma cama existente no repositório.
        /// </summary>
        /// <param name="bed">
        /// A entidade <see cref="Domain.Models.Bed"/> contendo os dados atualizados.
        /// </param>
        /// <returns>
        /// Uma tarefa assíncrona que representa a operação de atualização.
        /// </returns>
        /// <remarks>
        /// Este método deve sobrescrever os valores atuais da cama no banco de dados
        /// com os novos dados fornecidos. A responsabilidade de validar regras de 
        /// negócio antes da atualização é da camada de aplicação.
        /// </remarks>
        Task UpdateAsync(Domain.Models.Bed bed);

        /// Este método deve ser usado para recuperar o estado atual de uma cama 
        /// antes de realizar operações de atualização, verificação de disponibilidade 
        /// ou qualquer outra regra de negócio relacionada ao seu status.
        /// 
        /// A responsabilidade de validar se a cama existe e como tratar o caso de 
        /// retorno nulo pertence à camada de aplicação.
        /// </remarks>
        Task<Domain.Models.Bed?> GetByIdAsync(Guid bedId);
    }
}
