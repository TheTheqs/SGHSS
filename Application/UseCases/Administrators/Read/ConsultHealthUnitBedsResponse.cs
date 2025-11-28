// Application/UseCases/Administrators/Read/ConsultHealthUnitBedsResponse.cs

using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Administrators.Read
{
    /// <summary>
    /// Representa o resultado da consulta de camas de uma unidade de saúde.
    /// </summary>
    public sealed class ConsultHealthUnitBedsResponse
    {
        /// <summary>
        /// ID da unidade de saúde consultada.
        /// </summary>
        public Guid HealthUnitId { get; }

        /// <summary>
        /// Coleção de camas resultante da aplicação dos filtros.
        /// </summary>
        public IReadOnlyCollection<Bed> Beds { get; }

        /// <summary>
        /// Cria uma nova resposta para a consulta de camas.
        /// </summary>
        /// <param name="healthUnitId">ID da unidade consultada.</param>
        /// <param name="beds">Coleção de camas retornada.</param>
        public ConsultHealthUnitBedsResponse(Guid healthUnitId, IReadOnlyCollection<Bed> beds)
        {
            HealthUnitId = healthUnitId;
            Beds = beds;
        }
    }
}
