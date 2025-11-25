// Application/UseCases/EletronicPrescriptions/Issue/IssueEletronicPrescriptionRequest.cs


namespace SGHSS.Application.UseCases.EletronicPrescriptions.Issue
{
    /// <summary>
    /// Representa os dados necessários para emissão de uma prescrição eletrônica
    /// para um paciente.
    /// </summary>
    /// <remarks>
    /// Este request utiliza apenas identificadores das entidades de domínio
    /// (paciente, profissional, unidade de saúde e consulta), além de dados
    /// primitivos para as instruções e assinatura ICP. A criação do Value Object
    /// <c>IcpSignature</c> e o preenchimento de <see cref="Domain.Models.EletronicPrescription.CreatedAt"/>
    /// serão realizados no próprio caso de uso.
    /// </remarks>
    public sealed class IssueEletronicPrescriptionRequest
    {
        /// <summary>
        /// Identificador do paciente para o qual a prescrição será emitida.
        /// </summary>
        public Guid PatientId { get; init; }

        /// <summary>
        /// Identificador do profissional responsável pela emissão da prescrição.
        /// </summary>
        public Guid ProfessionalId { get; init; }

        /// <summary>
        /// Identificador da unidade de saúde em que a prescrição foi emitida.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Identificador da consulta associada a esta prescrição.
        /// </summary>
        /// <remarks>
        /// O modelo de domínio exige uma <see cref="Domain.Models.Appointment"/> associada,
        /// portanto este campo é obrigatório.
        /// </remarks>
        public Guid AppointmentId { get; init; }

        /// <summary>
        /// Data e hora até a qual a prescrição é considerada válida.
        /// </summary>
        /// <remarks>
        /// A data de criação (<c>CreatedAt</c>) será definida pelo caso de uso,
        /// normalmente usando o horário atual em UTC.
        /// </remarks>
        public DateTimeOffset ValidUntil { get; init; }

        /// <summary>
        /// Instruções de uso, posologia e orientações clínicas a serem incluídas na prescrição.
        /// </summary>
        public string Instructions { get; init; } = string.Empty;

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
