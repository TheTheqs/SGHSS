// Application/UseCases/Administrators/Read/ConsultHealthUnitBedsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Administrators.Read
{
    /// <summary>
    /// Consulta as camas de uma unidade de saúde, permitindo filtrar opcionalmente
    /// por tipo e status. Caso nenhum filtro seja especificado, todas as camas da
    /// unidade são retornadas.
    /// </summary>
    public class ConsultHealthUnitBedsUseCase
    {
        private readonly IHealthUnitRepository _healthUnitRepository;

        public ConsultHealthUnitBedsUseCase(IHealthUnitRepository healthUnitRepository)
        {
            _healthUnitRepository = healthUnitRepository;
        }

        /// <summary>
        /// Obtém as camas vinculadas à unidade de saúde informada, aplicando filtros opcionais.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o ID da unidade e filtros opcionais de tipo e status.
        /// </param>
        /// <returns>
        /// Um <see cref="ConsultHealthUnitBedsResponse"/> com a coleção filtrada de camas.
        /// </returns>
        /// <exception cref="InvalidOperationException">Lançada quando a unidade não existe.</exception>
        public async Task<ConsultHealthUnitBedsResponse> Handle(ConsultHealthUnitBedsRequest request)
        {
            // 1. Busca a unidade junto com as camas
            HealthUnit? unit = await _healthUnitRepository.GetByIdAsync(request.HealthUnitId);

            if (unit is null)
                throw new InvalidOperationException("A unidade de saúde informada não existe.");

            // 2. Começa com todas as camas
            IEnumerable<Bed> beds = unit.Beds;

            // 3. Aplica filtros somente quando forem especificados
            if (request.Type.HasValue)
                beds = beds.Where(b => b.Type == request.Type.Value);

            if (request.Status.HasValue)
                beds = beds.Where(b => b.Status == request.Status.Value);

            var bedDtos = unit.Beds
                .Select(b => new BedDto
                {
                    BedId = b.Id,
                    BedNumber = b.BedNumber,
                    Type = b.Type,
                    Status = b.Status
                })
                .ToList();

            return new ConsultHealthUnitBedsResponse(request.HealthUnitId, bedDtos);
        }
    }
}
