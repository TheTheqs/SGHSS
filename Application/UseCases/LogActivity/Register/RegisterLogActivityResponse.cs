// Application/UseCases/LogActivities/Register/RegisterLogActivityResponse.cs

namespace SGHSS.Application.UseCases.LogActivities.Register
{
    /// <summary>
    /// Representa o retorno após o registro de uma atividade de log. Usada apenas para testes.
    /// </summary>
    public sealed class RegisterLogActivityResponse
    {
        /// <summary>
        /// Identificador único do log registrado.
        /// </summary>
        public Guid LogActivityId { get; init; }
    }
}
