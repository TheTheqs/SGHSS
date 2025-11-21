// Tests/TestData/Models/AdministratorGenerator.cs

using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Domain.Enums;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.ValueObjects;

namespace SGHSS.Tests.TestData.Models
{
    namespace SGHSS.Tests.TestData.Administrators
    {
        /// <summary>
        /// Classe utilitária para geração de dados de entrada de administrador
        /// (RegisterAdministratorRequest) com valores válidos para uso em testes.
        /// </summary>
        public static class AdministratorGenerator
        {
            private static readonly Random Rng = new();

            /// <summary>
            /// Gera um RegisterAdministratorRequest completo com dados fictícios,
            /// porém válidos de acordo com as regras dos Value Objects.
            /// </summary>
            /// <param name="providedEmail">
            /// E-mail opcional fornecido diretamente pelo teste.
            /// Caso não seja informado, um e-mail válido é gerado automaticamente.
            /// </param>
            /// <param name="providedAccessLevel">
            /// AccessLevel opcional definido pelo teste.
            /// Caso não seja informado, o nível padrão será AccessLevel.Basic.
            /// </param>
            /// <returns>Instância de RegisterAdministratorRequest preenchida.</returns>
            public static RegisterAdministratorRequest GenerateAdministrator(
                string providedEmail = "",
                AccessLevel? providedAccessLevel = null
            )
            {
                // Nome completo do administrador
                string fullName = NameGenerator.GetFullName();

                // Email baseado no nome
                string email = string.IsNullOrWhiteSpace(providedEmail)
                    ? EmailGenerator.GenerateEmail(fullName)
                    : providedEmail;

                // Telefone válido (string já normalizada)
                string phone = PhoneGenerator.GeneratePhone();

                // Access level fornecido ou default
                AccessLevel accessLevel = providedAccessLevel ?? AccessLevel.Basic;

                return new RegisterAdministratorRequest
                {
                    Name = fullName,
                    Email = email,
                    Phone = phone,
                    AccessLevel = accessLevel,

                    // Consents permanece default (lista vazia)
                    // para permitir flexibilidade nos testes.
                };
            }
        }
    }
}
