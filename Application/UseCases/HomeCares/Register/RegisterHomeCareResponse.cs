// Application/UseCases/HomeCares/Register/RegisterHomeCareResponse.cs

namespace SGHSS.Application.UseCases.HomeCares.Register
{
    /// <summary>
    /// Representa a resposta retornada após o registro bem-sucedido
    /// de um atendimento de home care.
    /// </summary>
    /// <remarks>
    /// Fornece o identificador único do registro criado, permitindo
    /// consultas e operações subsequentes sobre o atendimento.
    /// </remarks>
    public sealed class RegisterHomeCareResponse
    {
        /// <summary>
        /// Identificador único do atendimento de home care recém-registrado.
        /// </summary>
        public Guid HomeCareId { get; init; }
    }
}
