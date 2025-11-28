// Application/UseCases/Common/EntityDto.cs

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// DTO genérico representando uma entidade mínima
    /// composta apenas por identificador e nome.
    /// </summary>
    public sealed class EntityDto
    {
        /// <summary>
        /// Identificador único da entidade.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Nome associado à entidade.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Cria um novo EntityDto.
        /// </summary>
        /// <param name="id">Identificador da entidade.</param>
        /// <param name="name">Nome da entidade.</param>
        public EntityDto(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
