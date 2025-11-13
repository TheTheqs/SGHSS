// Domain/Models/User.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um usuário abstrato com informações de identificação, dados de contato, 
    /// status e os consentimentos associados.
    /// </summary>
    /// <remarks>
    /// Esta classe serve como base para entidades relacionadas a usuários, fornecendo propriedades
    /// comuns como ID, nome, e-mail, telefone e status. Também gerencia a coleção de consentimentos
    /// do usuário. As classes derivadas devem implementar comportamentos ou propriedades adicionais
    /// específicos de seus respectivos contextos.
    /// </remarks>
    public abstract class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Email Email { get; set; }
        public Phone Phone { get; set; }
        public UserStatus Status { get; set; }

        // Relacionamentos
        public ICollection<Consent> Consents { get; set; } = new List<Consent>();

        // Construtor padrão
        public User() { }

        public Consent? GetActiveConsent(ConsentScope scope)
        {
            return Consents
                .Where(c => c.Scope == scope && c.Status == ConsentStatus.Active)
                .FirstOrDefault();
        }
    }
}
