// Application/UseCases/DigitalMedicalCertificates/GetPatientDigitalMedicalCertificates.cs


using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.DigitalMedicalCertificates.Read
{
    /// <summary>
    /// Representa o resultado da consulta de atestados médicos digitais
    /// associados a um paciente específico.
    /// </summary>
    public sealed class GetPatientMedicalCertificatesResponse
    {
        /// <summary>
        /// Identificador do paciente cujos atestados foram consultados.
        /// </summary>
        public Guid PatientId { get; init; }

        /// <summary>
        /// Coleção de atestados médicos digitais emitidos para o paciente,
        /// representados em formato resumido.
        /// </summary>
        public IReadOnlyList<DigitalMedicalCertificateDto> Certificates { get; init; }
            = Array.Empty<DigitalMedicalCertificateDto>();
    }
}
