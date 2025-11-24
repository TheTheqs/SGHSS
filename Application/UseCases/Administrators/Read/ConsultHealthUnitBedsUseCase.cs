// Application/UseCases/Administrators/Read/ConsultHealthUnitBedsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
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
        /// <param name="healthUnitId">ID da unidade de saúde de onde as camas serão consultadas.</param>
        /// <param name="type">Tipo da cama (opcional).</param>
        /// <param name="status">Status da cama (opcional).</param>
        /// <returns>Coleção filtrada de camas da unidade.</returns>
        /// <exception cref="InvalidOperationException">Lançada quando a unidade não existe.</exception>
        public async Task<ICollection<Bed>> Handle(Guid healthUnitId, BedType? type = null, BedStatus? status = null)
        {
            // 1. Busca a unidade junto com as camas
            HealthUnit? unit = await _healthUnitRepository.GetByIdAsync(healthUnitId);

            if (unit is null)
                throw new InvalidOperationException("A unidade de saúde informada não existe.");

            // 2. Começa com todas as camas
            IEnumerable<Bed> beds = unit.Beds;

            // 3. Aplica filtros somente quando forem especificados
            if (type.HasValue)
                beds = beds.Where(b => b.Type == type.Value);

            if (status.HasValue)
                beds = beds.Where(b => b.Status == status.Value);

            return beds.ToList();
        }
    }
}
