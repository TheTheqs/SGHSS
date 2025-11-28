// Application/UseCases/Administrators/Read/ConsultHealthUnitBedsRequest.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Administrators.Read
{
    /// <summary>
    /// Representa os dados necessários para consultar as camas de uma unidade de saúde,
    /// permitindo filtros opcionais por tipo e status.
    /// </summary>
    public sealed class ConsultHealthUnitBedsRequest
    {
        /// <summary>
        /// ID da unidade de saúde cujas camas serão consultadas.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Tipo da cama a ser filtrado (opcional).
        /// Quando nulo, não aplica filtro por tipo.
        /// </summary>
        public BedType? Type { get; init; }

        /// <summary>
        /// Status da cama a ser filtrado (opcional).
        /// Quando nulo, não aplica filtro por status.
        /// </summary>
        public BedStatus? Status { get; init; }
    }
}
