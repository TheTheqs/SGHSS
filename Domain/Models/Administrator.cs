// Domain/Models/Administrator.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um administrador do sistema, incluindo seu nível de acesso e permissões.
    /// </summary>
    public class Administrator : User
    {
        public AccessLevel AccessLevel { get; set; }

        // Construtor padrão
        public Administrator() { }
    }
}
