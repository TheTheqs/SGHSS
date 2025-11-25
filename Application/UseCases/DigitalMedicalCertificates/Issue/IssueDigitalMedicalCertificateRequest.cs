// Application/UseCases/DigitalMedicalCertificates/Issue/IssueDigitalMedicalCertificateRequest.cs

namespace SGHSS.Application.UseCases.DigitalMedicalCertificates.Issue
{
    /// <summary>
    /// Representa os dados necessários para emissão de um atestado médico digital
    /// para um paciente.
    /// </summary>
    /// <remarks>
    /// Este request utiliza apenas identificadores das entidades de domínio
    /// (paciente, profissional, unidade de saúde e consulta), além de dados
    /// primitivos para a descrição e assinatura ICP. A criação do Value Object
    /// <c>IcpSignature</c> e o preenchimento de <see cref="Domain.Models.DigitalMedicalCertificate.IssuedAt"/>
    /// serão realizados no próprio caso de uso.
    /// </remarks>
    public sealed class IssueDigitalMedicalCertificateRequest
    {
        /// <summary>
        /// Identificador do paciente para o qual o atestado será emitido.
        /// </summary>
        public Guid PatientId { get; init; }

        /// <summary>
        /// Identificador do profissional responsável pela emissão do atestado.
        /// </summary>
        public Guid ProfessionalId { get; init; }

        /// <summary>
        /// Identificador da unidade de saúde em que o atestado foi emitido.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Identificador da consulta associada a este atestado.
        /// </summary>
        /// <remarks>
        /// O modelo de domínio exige uma <see cref="Domain.Models.Appointment"/> associada,
        /// portanto este campo é obrigatório.
        /// </remarks>
        public Guid AppointmentId { get; init; }

        /// <summary>
        /// Data e hora até a qual o atestado é considerado válido.
        /// </summary>
        /// <remarks>
        /// A data de emissão (<c>IssuedAt</c>) será definida pelo caso de uso,
        /// normalmente usando o horário atual em UTC.
        /// </remarks>
        public DateTimeOffset ValidUntil { get; init; }

        /// <summary>
        /// Recomendações, observações ou justificativas clínicas a serem incluídas no atestado.
        /// </summary>
        public string Recommendations { get; init; } = string.Empty;

        /// <summary>
        /// Conteúdo bruto da assinatura digital ICP-Brasil a ser utilizado
        /// para construção do Value Object <c>IcpSignature</c>.
        /// </summary>
        /// <remarks>
        /// O formato exato (por exemplo, token, blob assinado, representação base64)
        /// é tratado na camada de domínio. Aqui o dado é mantido como string primitiva.
        /// </remarks>
        public string IcpSignatureRaw { get; init; } = string.Empty;
    }
}
