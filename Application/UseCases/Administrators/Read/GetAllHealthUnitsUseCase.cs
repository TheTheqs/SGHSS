// Application/UseCases/Administrators/Read/GetAllHealthUnitsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Administrators.Read
{
    /// <summary>
    /// Caso de uso responsável por listar todas as unidades de saúde
    /// em formato simplificado (ID e Nome).
    /// </summary>
    public sealed class GetAllHealthUnitsUseCase
    {
        private readonly IHealthUnitRepository _healthUnitRepository;

        public GetAllHealthUnitsUseCase(IHealthUnitRepository healthUnitRepository)
        {
            _healthUnitRepository = healthUnitRepository;
        }

        /// <summary>
        /// Recupera todas as unidades de saúde registradas,
        /// projetando-as em uma lista de <see cref="EntityDto"/>.
        /// </summary>
        /// <returns>
        /// Um <see cref="GetAllResponse"/> contendo a coleção de entidades simplificadas.
        /// </returns>
        public async Task<GetAllResponse> Handle()
        {
            var units = await _healthUnitRepository.GetAllAsync();

            var items = units
                .Select(u => new EntityDto(u.Id, u.Name))
                .ToList();

            return new GetAllResponse(items);
        }
    }
}
