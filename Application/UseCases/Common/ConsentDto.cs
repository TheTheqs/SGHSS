// Application/UseCases/Common/ConsentDto.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa os detalhes do consentimento de um usuário, incluindo escopo, versão, canal, data e um hash de verificação.
    /// </summary>
    public class ConsentDto
    {
        /// <summary>
        /// Escopo do consentimento concedido pelo usuário.
        /// </summary>
        public ConsentScope Scope { get; init; }

        /// <summary>
        /// Versão dos termos aos quais o usuário consentiu.
        /// </summary>
        public string TermVersion { get; init; } = string.Empty;

        /// <summary>
        /// Canal por meio do qual o consentimento foi fornecido.
        /// </summary>
        public ConsentChannel Channel { get; init; }

        /// <summary>
        /// Data e hora em que o consentimento foi concedido.
        /// </summary>
        public DateTimeOffset ConsentDate { get; init; }

        /// <summary>
        /// Data e Hora em que o consentimento foi revogado, se aplicável. Null se não revogado.
        /// </summary>
        public DateTimeOffset? RevocationDate { get; init; }

        /// <summary>
        /// Hash de verificação utilizado para garantir a integridade dos termos de consentimento.
        /// </summary>
        public string TermHash { get; init; } = string.Empty;
    }
}
