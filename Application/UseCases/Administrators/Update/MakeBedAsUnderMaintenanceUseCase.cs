// Application/UseCases/Administrators/Update/MakeBedAsUnderMaintenanceUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Administrators.Update
{
    /// <summary>
    /// Caso de uso responsável por colocar uma cama em estado de manutenção,
    /// permitindo que administradores sinalizem que ela está temporariamente indisponível.
    /// </summary>
    /// <remarks>
    /// Este caso de uso garante que somente camas atualmente disponíveis 
    /// possam ser movidas para o estado <see cref="BedStatus.UnderMaintenance"/>.
    /// Caso a cama já esteja em manutenção, ocupada ou reservada, uma exceção
    /// será lançada para impedir inconsistências na lógica operacional.
    /// </remarks>
    public class MakeBedAsUnderMaintenanceUseCase
    {
        private readonly IBedRepository _bedRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso para definir uma cama como "em manutenção".
        /// </summary>
        /// <param name="bedRepository">
        /// Repositório de camas utilizado para recuperar e atualizar a entidade.
        /// </param>
        public MakeBedAsUnderMaintenanceUseCase(IBedRepository bedRepository)
        {
            _bedRepository = bedRepository;
        }

        /// <summary>
        /// Define a cama especificada como "em manutenção", caso ela esteja disponível.
        /// </summary>
        /// <param name="bedId">O identificador único da cama a ser modificada.</param>
        /// <returns>Uma tarefa assíncrona representando a operação.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Lançada quando nenhuma cama com o ID informado é encontrada.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a cama não está em estado disponível.
        /// </exception>
        public async Task Handle(Guid bedId)
        {
            // Buscar cama no repositório
            Bed? bed = await _bedRepository.GetByIdAsync(bedId);

            if (bed is null)
                throw new KeyNotFoundException("Nenhuma cama foi encontrada com o ID informado.");

            // Apenas camas disponíveis podem entrar em manutenção
            if (bed.Status != BedStatus.Available)
                throw new InvalidOperationException("A cama só pode ser colocada em manutenção se estiver disponível.");

            // Alterar estado para UnderMaintenance
            bed.Status = BedStatus.UnderMaintenance;

            // Persistir atualização
            await _bedRepository.UpdateAsync(bed);
        }
    }
}
