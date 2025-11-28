// Application/UseCase/Common/GetAllResponse.cs

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Resposta padrão para endpoints GetAll,
    /// contendo apenas uma lista de <see cref="EntityDto"/>.
    /// </summary>
    public sealed class GetAllResponse
    {
        /// <summary>
        /// Coleção de entidades mínimas retornadas.
        /// </summary>
        public IReadOnlyList<EntityDto> Items { get; }

        /// <summary>
        /// Cria uma nova resposta GetAll.
        /// </summary>
        /// <param name="items">Lista de entidades DTO.</param>
        public GetAllResponse(IReadOnlyList<EntityDto> items)
        {
            Items = items;
        }
    }
}
