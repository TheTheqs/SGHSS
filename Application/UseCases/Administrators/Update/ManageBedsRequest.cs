// Application/UseCases/Administrators/Update/ManageBedsRequest.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Administrators.Update
{
    /// <summary>
    /// Representa os dados necessários para adicionar ou remover leitos
    /// em uma unidade de saúde existente.
    /// </summary>
    /// <remarks>
    /// Esta requisição é utilizada pelo caso de uso de gerenciamento de leitos
    /// da unidade, permitindo tanto incrementos quanto reduções na capacidade.
    /// A operação executada é determinada pela propriedade <see cref="IsAdding"/>.
    /// Leitos são tratados como itens pertencentes ao agregado da HealthUnit,
    /// funcionando como uma capacidade estrutural da unidade.
    /// </remarks>
    public sealed class ManageBedsRequest
    {
        /// <summary>
        /// Identificador da unidade de saúde onde ocorrerá a modificação de leitos.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Quantidade total atual de leitos da unidade, utilizada para validações.
        /// </summary>
        public int TotalBeds { get; init; }

        /// <summary>
        /// Modelo de leito que será adicionado ou removido da unidade.
        /// </summary>
        public BedDto Bed { get; init; } = null!;

        /// <summary>
        /// Quantidade de leitos a adicionar ou remover da unidade.
        /// </summary>
        public int Quantity { get; init; }

        /// <summary>
        /// Define se a operação será de adição (<c>true</c>)
        /// ou remoção (<c>false</c>) de leitos.
        /// </summary>
        public bool IsAdding { get; init; }
    }
}
