// Application/UseCases/Administrators/Update/ManageBedsResponse.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Administrators.Update
{
    /// <summary>
    /// Representa o resultado da operação de gerenciamento de leitos
    /// (adição ou remoção) em uma unidade de saúde.
    /// </summary>
    /// <remarks>
    /// Após a execução da operação, esta resposta retorna o identificador
    /// da unidade afetada e o estado atualizado da lista de leitos
    /// no formato seguro <see cref="BedDto"/>, evitando ciclos de serialização.
    /// </remarks>
    public class ManageBedsResponse
    {
        /// <summary>
        /// Identificador da unidade de saúde que teve seus leitos modificados.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Lista resultante de leitos após a operação de adição ou remoção,
        /// representados como DTOs para evitar retorno de entidades de domínio.
        /// </summary>
        public IReadOnlyList<BedDto> Beds { get; init; } = new List<BedDto>();

        /// <summary>
        /// Cria uma nova instância da resposta de gerenciamento de leitos.
        /// </summary>
        /// <param name="healthUnitId">O identificador da unidade afetada.</param>
        /// <param name="beds">Lista atualizada de leitos no formato DTO.</param>
        public ManageBedsResponse(Guid healthUnitId, IReadOnlyList<BedDto> beds)
        {
            HealthUnitId = healthUnitId;
            Beds = beds;
        }
    }
}
