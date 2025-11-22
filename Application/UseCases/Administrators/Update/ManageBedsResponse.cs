// Application/UseCases/Administrators/Update/ManageBedsResponse.cs


using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Administrators.Update
{
    /// <summary>
    /// Representa o resultado da operação de gerenciamento de leitos
    /// (adição ou remoção) em uma unidade de saúde.
    /// </summary>
    /// <remarks>
    /// Após a execução da operação, esta resposta retorna o identificador
    /// da unidade afetada e o estado atualizado da lista de leitos,
    /// permitindo que camadas superiores exibam ou validem as mudanças.
    /// </remarks>
    public class ManageBedsResponse
    {
        /// <summary>
        /// Identificador da unidade de saúde que teve seus leitos modificados.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Lista resultante de leitos após a operação de adição ou remoção.
        /// Representa o estado atualizado da unidade.
        /// </summary>
        public IReadOnlyList<Bed> Beds { get; init; } = new List<Bed>();

        /// <summary>
        /// Cria uma nova instância da resposta de gerenciamento de leitos.
        /// </summary>
        /// <param name="healthUnitId">O identificador da unidade afetada.</param>
        /// <param name="beds">Lista atualizada de leitos.</param>
        public ManageBedsResponse(Guid healthUnitId, IReadOnlyList<Bed> beds)
        {
            HealthUnitId = healthUnitId;
            Beds = beds;
        }
    }
}
