// Tests/Domain/DigitalMedicalCertificates.cs

using SGHSS.Application.UseCases.DigitalMedicalCertificates.Issue;

namespace SGHSS.Tests.TestData.Models
{


    /// <summary>
    /// Classe utilitária para geração de dados de entrada de emissão de atestado médico digital
    /// (<see cref="IssueDigitalMedicalCertificateRequest"/>) com valores fictícios,
    /// porém válidos de acordo com as regras de domínio.
    /// </summary>
    public static class DigitalMedicalCertificateRequestGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um <see cref="IssueDigitalMedicalCertificateRequest"/> completo com dados fictícios,
        /// permitindo a sobrescrita de campos específicos para cenários de teste.
        /// </summary>
        /// <param name="providedPatientId">
        /// Identificador de paciente a ser utilizado. Caso seja <c>null</c>, um novo <see cref="Guid"/> será gerado.
        /// </param>
        /// <param name="providedProfessionalId">
        /// Identificador de profissional a ser utilizado. Caso seja <c>null</c>, um novo <see cref="Guid"/> será gerado.
        /// </param>
        /// <param name="providedHealthUnitId">
        /// Identificador de unidade de saúde a ser utilizado. Caso seja <c>null</c>, um novo <see cref="Guid"/> será gerado.
        /// </param>
        /// <param name="providedAppointmentId">
        /// Identificador de consulta a ser utilizado. Caso seja <c>null</c>, um novo <see cref="Guid"/> será gerado.
        /// </param>
        /// <param name="providedValidUntil">
        /// Data de validade do atestado. Se não informada, será gerada uma data futura (entre 1 e 14 dias à frente).
        /// </param>
        /// <param name="providedRecommendations">
        /// Recomendações clínicas a serem incluídas no atestado. Se não informada ou vazia,
        /// uma recomendação fictícia será gerada.
        /// </param>
        /// <param name="providedIcpSignatureRaw">
        /// Assinatura ICP bruta. Se não informada ou vazia, será gerada uma string Base64 válida
        /// para o Value Object <c>IcpSignature</c>.
        /// </param>
        /// <returns>
        /// Instância preenchida de <see cref="IssueDigitalMedicalCertificateRequest"/> pronta para uso em testes.
        /// </returns>
        public static IssueDigitalMedicalCertificateRequest Generate(
            Guid? providedPatientId = null,
            Guid? providedProfessionalId = null,
            Guid? providedHealthUnitId = null,
            Guid? providedAppointmentId = null,
            DateTimeOffset? providedValidUntil = null,
            string? providedRecommendations = null,
            string? providedIcpSignatureRaw = null
        )
        {
            Guid patientId = providedPatientId ?? Guid.NewGuid();
            Guid professionalId = providedProfessionalId ?? Guid.NewGuid();
            Guid healthUnitId = providedHealthUnitId ?? Guid.NewGuid();
            Guid appointmentId = providedAppointmentId ?? Guid.NewGuid();

            DateTimeOffset validUntil = providedValidUntil ?? GenerateFutureValidUntil();

            string recommendations = string.IsNullOrWhiteSpace(providedRecommendations)
                ? GenerateRecommendations()
                : providedRecommendations!;

            string icpSignatureRaw = string.IsNullOrWhiteSpace(providedIcpSignatureRaw)
                ? GenerateValidIcpSignatureRaw()
                : providedIcpSignatureRaw!;

            return new IssueDigitalMedicalCertificateRequest
            {
                PatientId = patientId,
                ProfessionalId = professionalId,
                HealthUnitId = healthUnitId,
                AppointmentId = appointmentId,
                ValidUntil = validUntil,
                Recommendations = recommendations,
                IcpSignatureRaw = icpSignatureRaw
            };
        }

        /// <summary>
        /// Gera uma data de validade futura para o atestado (entre 1 e 14 dias à frente, em UTC).
        /// </summary>
        private static DateTimeOffset GenerateFutureValidUntil()
        {
            int daysAhead = Rng.Next(1, 15); // 1 a 14 dias
            return DateTimeOffset.UtcNow.AddDays(daysAhead);
        }

        /// <summary>
        /// Gera uma recomendação clínica fictícia e verossímil para o atestado.
        /// </summary>
        private static string GenerateRecommendations()
        {
            string[] samples =
            {
                    "Paciente deve permanecer em repouso absoluto por 3 dias.",
                    "Recomenda-se afastamento das atividades laborais por 5 dias.",
                    "Paciente liberado apenas para atividades leves, sem esforço físico.",
                    "Recomenda-se retorno ambulatorial em 7 dias ou se houver piora do quadro.",
                    "Manter uso da medicação prescrita e evitar exposição a esforços.",
                    "Paciente deve evitar dirigir e operar máquinas nas próximas 48 horas."
                };

            return samples[Rng.Next(samples.Length)];
        }

        /// <summary>
        /// Gera uma string Base64 representando uma estrutura DER mínima,
        /// válida para o Value Object <c>IcpSignature</c>.
        /// </summary>
        /// <remarks>
        /// A string representa um ASN.1 SEQUENCE (0x30) com conteúdo simples,
        /// garantindo que passe pelas validações de DER/PKCS#7 do VO.
        /// </remarks>
        private static string GenerateValidIcpSignatureRaw()
        {
            // Bytes: 30 03 01 01 FF
            // 0x30 (SEQUENCE), comprimento 0x03, BOOLEAN TRUE
            // Base64 canônica: "MAMBAf8="
            return "MAMBAf8=";
        }
    }

}
