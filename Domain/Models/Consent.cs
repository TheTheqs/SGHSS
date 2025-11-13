using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um registro de consentimento do usuário, incluindo seu escopo, status e metadados associados.
    /// </summary>
    /// <remarks>
    /// Uma instância de Consent encapsula as informações sobre a concordância de um usuário com determinados termos,
    /// incluindo a versão dos termos aceitos, o canal por meio do qual o consentimento foi concedido e os respectivos
    /// carimbos de data e hora. O status do consentimento reflete se ele está atualmente ativo ou se foi revogado.
    /// Esta classe é normalmente utilizada para rastrear e gerenciar consentimentos de usuários em cenários de
    /// conformidade e auditoria.
    /// </remarks>
    public class Consent
    {
        public Guid Id { get; set; }
        public ConsentScope Scope { get; set; }
        public required string TermVersion { get; set; }
        public ConsentChannel Channel { get; set; }
        public DateTimeOffset ConsentDate { get; set; }

        // null se nunca revogou
        public DateTimeOffset? RevocationDate { get; set; } = null;

        // Propriedade calculada, não armazenada no banco
        public ConsentStatus Status
        {
            get
            {
                return RevocationDate == null ? ConsentStatus.Active : ConsentStatus.Inactive;
            }
        }

        public HashDigest TermHash { get; set; }

        // Construtor padrão
        public Consent() { }
    }
}
