// Tests/TestData/Models/HospitalizationGenerator.cs

using SGHSS.Application.UseCases.Administrators.Update;

namespace SGHSS.Tests.TestData.Models
{
    /// <summary>
    /// Classe utilitária para geração de dados de entrada para o caso de uso
    /// de hospitalização de paciente (<see cref="HospitalizePatientRequest"/>).
    /// </summary>
    /// <remarks>
    /// Este gerador produz requests válidos por padrão, permitindo opcionalmente
    /// a customização de identificadores e do motivo da internação para montar
    /// cenários específicos de teste (como IDs inexistentes, razões vazias, etc.).
    /// </remarks>
    public static class HospitalizationGenerator
    {
        private static readonly Random Rng = new();

        /// <summary>
        /// Gera um <see cref="HospitalizePatientRequest"/> com valores padrão válidos,
        /// permitindo sobrescrever opcionalmente o identificador do paciente, o
        /// identificador da cama e o motivo da internação.
        /// </summary>
        /// <param name="patientId">
        /// Identificador do paciente. Caso não informado, será gerado um novo Guid.
        /// Normalmente, em testes de integração, este valor deve ser preenchido com
        /// o Id de um paciente realmente persistido.
        /// </param>
        /// <param name="bedId">
        /// Identificador da cama. Caso não informado, será gerado um novo Guid.
        /// Em cenários reais de teste, deve apontar para uma cama existente.
        /// </param>
        /// <param name="reason">
        /// Motivo da internação. Se não informado ou em branco, um motivo genérico
        /// será gerado automaticamente.
        /// </param>
        /// <returns>
        /// Instância de <see cref="HospitalizePatientRequest"/> pronta para uso em testes.
        /// </returns>
        public static HospitalizePatientRequest GenerateHospitalizeRequest(
            Guid? patientId = null,
            Guid? bedId = null,
            string? reason = null)
        {
            Guid resolvedPatientId = patientId ?? Guid.NewGuid();
            Guid resolvedBedId = bedId ?? Guid.NewGuid();
            string resolvedReason = string.IsNullOrWhiteSpace(reason)
                ? GenerateReason()
                : reason;

            return new HospitalizePatientRequest
            {
                PatientId = resolvedPatientId,
                BedId = resolvedBedId,
                Reason = resolvedReason
            };
        }

        /// <summary>
        /// Gera um texto genérico para o motivo da internação.
        /// </summary>
        private static string GenerateReason()
        {
            // Lista simples de motivos fictícios para testes
            string[] reasons =
            [
                "Internação para investigação diagnóstica.",
                "Internação eletiva para procedimento cirúrgico.",
                "Internação por descompensação clínica súbita.",
                "Internação para observação clínica.",
                "Internação por complicações pós-operatórias."
            ];

            int index = Rng.Next(reasons.Length);
            return reasons[index];
        }
    }
}
