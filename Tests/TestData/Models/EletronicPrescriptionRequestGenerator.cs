// Tests/Domain/EletronicPrescriptionRequestGenerator.cs

using SGHSS.Application.UseCases.EletronicPrescriptions.Issue;

namespace SGHSS.Tests.TestData.Models
{
    /// <summary>
    /// Classe utilitária para geração de dados de entrada de emissão de prescrição eletrônica
    /// (<see cref="IssueEletronicPrescriptionRequest"/>) com valores fictícios,
    /// porém válidos de acordo com as regras de domínio.
    /// </summary>
    public static class EletronicPrescriptionRequestGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um <see cref="IssueEletronicPrescriptionRequest"/> completo com dados fictícios,
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
        /// Data de validade da prescrição. Se não informada, será gerada uma data futura (entre 1 e 14 dias à frente).
        /// </param>
        /// <param name="providedInstructions">
        /// Instruções clínicas a serem incluídas na prescrição. Se não informada ou vazia,
        /// um conjunto fictício de instruções será gerado.
        /// </param>
        /// <param name="providedIcpSignatureRaw">
        /// Assinatura ICP bruta. Se não informada ou vazia, será gerada uma string Base64 válida
        /// para o Value Object <c>IcpSignature</c>.
        /// </param>
        /// <returns>
        /// Instância preenchida de <see cref="IssueEletronicPrescriptionRequest"/> pronta para uso em testes.
        /// </returns>
        public static IssueEletronicPrescriptionRequest Generate(
            Guid? providedPatientId = null,
            Guid? providedProfessionalId = null,
            Guid? providedHealthUnitId = null,
            Guid? providedAppointmentId = null,
            DateTimeOffset? providedValidUntil = null,
            string? providedInstructions = null,
            string? providedIcpSignatureRaw = null
        )
        {
            Guid patientId = providedPatientId ?? Guid.NewGuid();
            Guid professionalId = providedProfessionalId ?? Guid.NewGuid();
            Guid healthUnitId = providedHealthUnitId ?? Guid.NewGuid();
            Guid appointmentId = providedAppointmentId ?? Guid.NewGuid();

            DateTimeOffset validUntil = providedValidUntil ?? GenerateFutureValidUntil();

            string instructions = string.IsNullOrWhiteSpace(providedInstructions)
                ? GenerateInstructions()
                : providedInstructions!;

            string icpSignatureRaw = string.IsNullOrWhiteSpace(providedIcpSignatureRaw)
                ? GenerateValidIcpSignatureRaw()
                : providedIcpSignatureRaw!;

            return new IssueEletronicPrescriptionRequest
            {
                PatientId = patientId,
                ProfessionalId = professionalId,
                HealthUnitId = healthUnitId,
                AppointmentId = appointmentId,
                ValidUntil = validUntil,
                Instructions = instructions,
                IcpSignatureRaw = icpSignatureRaw
            };
        }

        /// <summary>
        /// Gera uma data de validade futura para a prescrição (entre 1 e 14 dias à frente, em UTC).
        /// </summary>
        private static DateTimeOffset GenerateFutureValidUntil()
        {
            int daysAhead = Rng.Next(1, 15); // 1 a 14 dias
            return DateTimeOffset.UtcNow.AddDays(daysAhead);
        }

        /// <summary>
        /// Gera instruções clínicas fictícias e verossímeis para a prescrição.
        /// </summary>
        private static string GenerateInstructions()
        {
            string[] samples =
            {
                "Tomar 1 comprimido a cada 8 horas por 5 dias.",
                "Aplicar a pomada na região afetada duas vezes ao dia.",
                "Ingerir 20 gotas diluídas em meio copo de água, 3 vezes ao dia.",
                "Tomar o medicamento apenas após as refeições principais.",
                "Manter hidratação adequada e evitar bebidas alcoólicas durante o tratamento.",
                "Suspender o uso em caso de reação alérgica e procurar atendimento médico."
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
