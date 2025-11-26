// Domain/ValueObjects/Password.cs

using System.ComponentModel.DataAnnotations;

namespace SGHSS.Domain.ValueObjects
{
    /// <summary>
    /// Representa a senha de um usuário, armazenada sempre como hash criptográfico.
    /// Garante validação mínima de segurança e oferece método de verificação.
    /// </summary>
    public class Password
    {
        /// <summary>
        /// Hash criptográfico da senha, nunca a senha pura.
        /// </summary>
        public string Hash { get; private set; } = string.Empty;

        /// <summary>
        /// Construtor protegido apenas para o EF.
        /// </summary>
        protected Password() { }

        private Password(string hash)
        {
            Hash = hash;
        }

        /// <summary>
        /// Cria uma instância de Password a partir de uma senha pura.
        /// Valida requisitos mínimos e gera o hash com BCrypt.
        /// </summary>
        public static Password Create(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
                throw new ValidationException("A senha não pode ser vazia.");

            if (plainTextPassword.Length < 6)
                throw new ValidationException("A senha deve ter pelo menos 6 caracteres.");

            if (!plainTextPassword.Any(char.IsLetter) || !plainTextPassword.Any(char.IsDigit))
                throw new ValidationException("A senha deve conter letras e números.");

            // Gera o hash com BCrypt (10 rounds)
            string hash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

            return new Password(hash);
        }

        /// <summary>
        /// Factory usado pelo EF para recriar o VO a partir do hash vindo do banco.
        /// NÃO gera hash novo, apenas encapsula o valor existente.
        /// </summary>
        internal static Password FromHash(string hash)
        {
            if (string.IsNullOrWhiteSpace(hash))
                throw new ValidationException("Hash de senha inválido.");

            return new Password(hash);
        }

        /// <summary>
        /// Verifica se a senha fornecida corresponde ao hash armazenado.
        /// </summary>
        public bool Verify(string plainTextPassword)
        {
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, Hash);
        }

        public override string ToString() => Hash;
    }
}
