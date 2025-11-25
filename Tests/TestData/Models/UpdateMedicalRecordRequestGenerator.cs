

using SGHSS.Application.UseCases.Patients.Update;

namespace SGHSS.Tests.TestData.Models
{
    namespace SGHSS.Tests.TestData.MedicalRecords
    {
        /// <summary>
        /// Classe utilitária responsável por gerar instâncias fictícias,
        /// porém válidas, de <see cref="UpdateMedicalRecordRequest"/> para uso em testes.
        /// </summary>
        public static class UpdateMedicalRecordRequestGenerator
        {
            private static readonly Random Rng = new();

            /// <summary>
            /// Gera um request completo e válido para atualização de prontuário,
            /// permitindo parâmetros opcionais para manipulação de cenários de teste.
            /// </summary>
            /// <param name="providedPatientId">
            /// Caso informado, será utilizado como ID do paciente;
            /// caso contrário, um novo paciente fictício será gerado.
            /// </param>
            /// <param name="providedProfessionalId">
            /// Caso informado, será utilizado como ID do profissional;
            /// caso contrário, um novo profissional fictício será gerado.
            /// </param>
            /// <param name="providedHealthUnitId">
            /// Caso informado, será utilizado como ID da unidade de saúde;
            /// caso contrário, uma unidade fictícia será gerada.
            /// </param>
            /// <param name="providedDescription">
            /// Descrição clínica da atualização; se não fornecida,
            /// uma descrição padrão aleatória será gerada.
            /// </param>
            /// <returns>
            /// Uma instância preenchida de <see cref="UpdateMedicalRecordRequest"/> pronta para uso nos testes.
            /// </returns>
            public static UpdateMedicalRecordRequest Generate(
                Guid? providedPatientId = null,
                Guid? providedProfessionalId = null,
                Guid? providedHealthUnitId = null,
                string? providedDescription = null
            )
            {
                // Se o teste quiser IDs específicos, usa os IDs dados.
                // Caso contrário, gera entidades válidas usando os outros generators.
                Guid patientId = providedPatientId ?? Guid.NewGuid();
                Guid professionalId = providedProfessionalId ?? Guid.NewGuid();
                Guid healthUnitId = providedHealthUnitId ?? Guid.NewGuid();

                string description = string.IsNullOrWhiteSpace(providedDescription)
                    ? GenerateRandomDescription()
                    : providedDescription!;

                return new UpdateMedicalRecordRequest
                {
                    PatientId = patientId,
                    ProfessionalId = professionalId,
                    HealthUnitId = healthUnitId,
                    AppointmentId = null,
                    Description = description
                };
            }

            /// <summary>
            /// Gera uma pequena frase fictícia de atualização clínica
            /// para simular anotações médicas verossímeis em testes.
            /// </summary>
            private static string GenerateRandomDescription()
            {
                string[] samples =
                {
                    "Paciente apresenta melhora significativa no quadro.",
                    "Realizada troca de curativo sem intercorrências.",
                    "Paciente relata dor leve ao movimento.",
                    "Sinais vitais estáveis; manter observação.",
                    "Iniciado antibiótico conforme protocolo.",
                    "Paciente orientado sobre cuidados domiciliares.",
                    "Sem alterações relevantes desde a última avaliação."
                };

                return samples[Rng.Next(samples.Length)];
            }
        }
    }
}
