// Application/Interfaces/Repositories/ILogActivityRepository.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.Interfaces.Repositories
{
    /// <summary>
    /// Define o contrato para operações de persistência relacionadas à entidade
    /// <see cref="LogActivity"/>, permitindo o registro e a consulta de eventos
    /// de auditoria ao longo de um intervalo de tempo.
    /// </summary>
    public interface ILogActivityRepository
    {
        /// <summary>
        /// Adiciona um novo registro de atividade ao repositório.
        /// </summary>
        /// <param name="logActivity">
        /// Instância de <see cref="LogActivity"/> a ser persistida.
        /// </param>
        /// <remarks>
        /// Este método deve ser utilizado sempre que uma ação relevante
        /// precisar ser registrada para fins de auditoria, segurança
        /// ou solução de problemas.
        /// </remarks>
        Task AddAsync(LogActivity logActivity);

        /// <summary>
        /// Recupera todos os registros de atividade dentro do intervalo de tempo informado.
        /// </summary>
        /// <param name="from">
        /// Data e hora inicial (inclusiva) do intervalo de consulta.
        /// </param>
        /// <param name="to">
        /// Data e hora final (exclusiva) do intervalo de consulta.
        /// </param>
        /// <returns>
        /// Uma coleção somente leitura contendo todas as instâncias de
        /// <see cref="LogActivity"/> cujo <c>Timestamp</c> esteja dentro do intervalo especificado.
        /// </returns>
        /// <remarks>
        /// O resultado deve ser ordenado por <see cref="LogActivity.Timestamp"/> em ordem crescente,
        /// facilitando a visualização cronológica dos eventos em relatórios de auditoria.
        /// </remarks>
        Task<IReadOnlyCollection<LogActivity>> GetByPeriodAsync(
            DateTimeOffset from,
            DateTimeOffset to);
    }
}
