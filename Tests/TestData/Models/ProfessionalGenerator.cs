// Tests/TestData/Models/ProfessionalGenerator.cs

using SGHSS.Application.UseCases.Common;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Tests.TestData.Common;
using SGHSS.Tests.TestData.ValueObjects;

namespace SGHSS.Tests.TestData.Models
{
    namespace SGHSS.Tests.TestData.Professionals
    {
        /// <summary>
        /// Classe utilitária para geração de dados de entrada de profissional
        /// (RegisterProfessionalRequest) com valores válidos para uso em testes.
        /// </summary>
        public static class ProfessionalGenerator
        {
            private static readonly Random Rng = new();

            /// <summary>
            /// Gera um RegisterProfessionalRequest completo com dados fictícios,
            /// porém válidos de acordo com as regras dos Value Objects.
            /// </summary>
            /// <param name="durationInMinutes">
            /// Duração padrão dos atendimentos, em minutos. Caso seja <c>null</c>, utiliza 30 minutos.
            /// </param>
            /// <param name="weeklyWindowsCount">
            /// Quantidade de janelas semanais a serem geradas para a SchedulePolicy. Padrão: 2.
            /// </param>
            /// <returns>Instância de RegisterProfessionalRequest preenchida.</returns>
            public static RegisterProfessionalRequest GenerateProfessional(
                int? durationInMinutes = null,
                int weeklyWindowsCount = 2,
                string providedEmail = "",
                string providedLicense = "")
            {
                // Nome completo do profissional
                string fullName = NameGenerator.GetFullName();

                // Email baseado no nome
                string email = string.IsNullOrWhiteSpace(providedEmail)
                    ? EmailGenerator.GenerateEmail(fullName)
                    : providedEmail;

                // Telefone válido
                string phone = PhoneGenerator.GeneratePhone();

                // Licença válida
                string license = string.IsNullOrWhiteSpace(providedLicense)
                    ? ProfessionalLicenseGenerator.GenerateLicense()
                    : providedLicense;

                // Especialidade fictícia
                string specialty = PickSpecialty();

                // Política de agendamento baseada nos parâmetros
                SchedulePolicyDto schedulePolicy =
                    SchedulePolicyGenerator.GeneratePolicy(durationInMinutes, weeklyWindowsCount);

                return new RegisterProfessionalRequest
                {
                    Name = fullName,
                    Email = email,
                    Phone = phone,
                    License = license,
                    Specialty = specialty,
                    SchedulePolicy = schedulePolicy,
                    // Consents intencionalmente deixado como lista vazia,
                    // para que os testes tenham liberdade de montar o contexto.
                    Consents = new List<ConsentDto>()
                };
            }

            /// <summary>
            /// Escolhe uma especialidade fictícia comum na área da saúde.
            /// </summary>
            private static string PickSpecialty()
            {
                var specialties = new List<string>
                {
                    "Clínico Geral",
                    "Cardiologia",
                    "Pediatria",
                    "Ginecologia",
                    "Ortopedia",
                    "Dermatologia",
                    "Psiquiatria",
                    "Endocrinologia",
                    "Geriatria",
                    "Neurologia",
                    "Enfermagem",
                    "Nutrição",
                    "Fisioterapia",
                    "Fonoaudiologia",
                    "Psicologia"
                };

                return specialties[Rng.Next(specialties.Count)];
            }
        }
    }
}
