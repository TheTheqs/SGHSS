// Tests/TestData/Models/PatientGenerator.cs

using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Domain.Enums;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.ValueObjects;


namespace SGHSS.Tests.TestData.Models
{
    namespace SGHSS.Tests.TestData.Patients
    {
        /// <summary>
        /// Classe utilitária para geração de dados de entrada de paciente
        /// (RegisterPatientRequest) com valores válidos para uso em testes.
        /// </summary>
        public static class PatientGenerator
        {
            private static readonly Random Rng = new();

            /// <summary>
            /// Gera um RegisterPatientRequest completo com dados fictícios,
            /// porém válidos de acordo com as regras dos Value Objects.
            /// </summary>
            /// <returns>Instância de RegisterPatientRequest preenchida.</returns>
            public static RegisterPatientRequest GeneratePatient(bool isUnderage = false, string providedCpf = "", string providedEmail = "")
            {
                // Nome completo do paciente
                string fullName = NameGenerator.GetFullName();

                // Email baseado no nome
                string email = string.IsNullOrWhiteSpace(providedEmail)
                    ? EmailGenerator.GenerateEmail(fullName)
                    : providedEmail;

                // CPF válido
                string cpf = string.IsNullOrWhiteSpace(providedCpf)
                    ? CpfGenerator.GenerateCpf(withMask: false)
                    : providedCpf;

                // Password
                string password = PasswordGenerator.GenerateRawPassword();

                // Telefone válido (string já normalizada para o VO de Phone)
                string phone = PhoneGenerator.GeneratePhone();

                // Data de nascimento (sempre maior de 18 anos)
                DateTimeOffset birthDate = isUnderage
                            ? DateTimeOffsetGenerator.GenerateRecentDate()
                            : DateTimeOffsetGenerator.GenerateBirthDate();

                // Endereço fictício, porém válido, via AddressDto
                AddressDto address = AddressGenerator.GenerateAddress();

                // Sexo sorteado a partir do enum Sex (mais robusto se o enum mudar)
                Sex sex = NameGenerator.InferSexFromName(fullName);

                // Contato de emergência opcional (às vezes null, às vezes nome gerado)
                string? emergencyContactName = GenerateEmergencyContactName();

                return new RegisterPatientRequest
                {
                    Name = fullName,
                    Email = email,
                    Password = password,
                    Phone = phone,
                    Cpf = cpf,
                    BirthDate = birthDate,
                    Sex = sex,
                    Address = address,
                    EmergencyContactName = emergencyContactName,
                    // Consents intencionalmente deixado como default (lista vazia),
                    // para que os testes tenham liberdade de montar o contexto.
                };
            }

            /// <summary>
            /// Gera um nome de contato de emergência opcional.
            /// Em parte dos casos retorna null, em outros um nome válido.
            /// </summary>
            private static string? GenerateEmergencyContactName()
            {
                // ~60% de chance de ter contato de emergência preenchido
                if (Rng.NextDouble() < 0.6)
                    return NameGenerator.GetFullName();

                return null;
            }
        }
    }

}
