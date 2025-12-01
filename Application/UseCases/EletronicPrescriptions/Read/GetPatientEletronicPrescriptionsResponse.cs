// Application/UseCases/EletronicPrescriptions/GetPatientEletronicPrescriptionsRequest.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.EletronicPrescriptions.Read
{
    /// <summary>
    /// Representa o resultado da consulta de prescrições eletrônicas
    /// associadas a um paciente específico.
    /// </summary>
    public sealed class GetPatientEletronicPrescriptionsResponse
    {
        /// <summary>
        /// Identificador do paciente cujas prescrições foram consultadas.
        /// </summary>
        public Guid PatientId { get; init; }

        /// <summary>
        /// Coleção de prescrições eletrônicas emitidas para o paciente,
        /// representadas em formato resumido.
        /// </summary>
        public IReadOnlyList<EletronicPrescriptionDto> Prescriptions { get; init; }
            = Array.Empty<EletronicPrescriptionDto>();
    }
}

