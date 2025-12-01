// Application/UseCases/DigitalMedicalCertificates/GetPatientMedicalCertificates.cs

namespace SGHSS.Application.UseCases.DigitalMedicalCertificates.Read
{
    /// <summary>
    /// Representa os dados necessários para consultar todos os
    /// atestados médicos digitais associados a um paciente.
    /// </summary>
    /// <remarks>
    /// Este request utiliza apenas o identificador do paciente.
    /// Regras de autorização e verificação de vínculo entre o usuário
    /// autenticado e o paciente devem ser tratadas na camada de controle.
    /// </remarks>
    public sealed class GetPatientMedicalCertificatesRequest
    {
        /// <summary>
        /// Identificador do paciente cujos atestados médicos digitais
        /// deverão ser consultados.
        /// </summary>
        public Guid PatientId { get; init; }
    }
}
