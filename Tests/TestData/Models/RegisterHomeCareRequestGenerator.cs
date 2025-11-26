// Tests/TestData/Models/RegisterHomeCareRequestGenerator.cs

using SGHSS.Application.UseCases.HomeCares.Register;

namespace SGHSS.Tests.TestData.Models
{
    /// <summary>
    /// Classe utilitária para geração de dados de entrada do caso de uso de registro
    /// de atendimentos de home care (<see cref="RegisterHomeCareRequest"/>),
    /// fornecendo valores fictícios porém válidos para cenários de teste.
    /// </summary>
    public static class RegisterHomeCareRequestGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um <see cref="RegisterHomeCareRequest"/> completo com dados fictícios
        /// para data e descrição, utilizando obrigatoriamente identificadores
        /// já existentes de paciente, profissional e unidade de saúde.
        /// </summary>
        /// <param name="patientId">
        /// Identificador do paciente que recebeu o atendimento de home care.
        /// </param>
        /// <param name="professionalId">
        /// Identificador do profissional que realizou o atendimento.
        /// </param>
        /// <param name="healthUnitId">
        /// Identificador da unidade de saúde responsável pelo serviço.
        /// </param>
        /// <param name="providedDate">
        /// Data e horário do atendimento. Caso não informado, uma data recente
        /// será gerada para uso em testes.
        /// </param>
        /// <param name="providedDescription">
        /// Descrição do atendimento realizado. Caso não informada ou vazia,
        /// uma descrição fictícia e coerente será criada.
        /// </param>
        /// <returns>
        /// Instância preenchida de <see cref="RegisterHomeCareRequest"/> pronta para uso em testes.
        /// </returns>
        public static RegisterHomeCareRequest Generate(
            Guid patientId,
            Guid professionalId,
            Guid healthUnitId,
            DateTimeOffset? providedDate = null,
            string? providedDescription = null
        )
        {
            DateTimeOffset date = providedDate ?? GenerateRecentDate();
            string description = string.IsNullOrWhiteSpace(providedDescription)
                ? GenerateDescription()
                : providedDescription!;

            return new RegisterHomeCareRequest
            {
                Date = date,
                Description = description,
                PatientId = patientId,
                ProfessionalId = professionalId,
                HealthUnitId = healthUnitId
            };
        }

        /// <summary>
        /// Gera uma data recente (nos últimos dias) para simular a realização
        /// de um atendimento de home care em cenários de teste.
        /// </summary>
        private static DateTimeOffset GenerateRecentDate()
        {
            // Últimos 30 dias em relação a agora
            int daysBack = Rng.Next(0, 30);
            int hour = Rng.Next(7, 21); // atendimento entre 07:00 e 20:59

            var baseDate = DateTimeOffset.Now.Date
                .AddDays(-daysBack)
                .AddHours(hour);

            return baseDate;
        }

        /// <summary>
        /// Gera uma descrição fictícia e coerente para o atendimento de home care.
        /// </summary>
        private static string GenerateDescription()
        {
            string[] samples =
            {
                "Atendimento domiciliar para avaliação de quadro clínico e ajuste de medicação.",
                "Visita de rotina para acompanhamento de sinais vitais e orientações ao cuidador.",
                "Atendimento de home care com foco em controle de dor e conforto do paciente.",
                "Consulta domiciliar para reavaliação pós-alta hospitalar.",
                "Visita técnica para acompanhamento de tratamento crônico em ambiente domiciliar.",
                "Atendimento domiciliar com registro de evolução e orientação sobre cuidados gerais."
            };

            return samples[Rng.Next(samples.Length)];
        }
    }
}
