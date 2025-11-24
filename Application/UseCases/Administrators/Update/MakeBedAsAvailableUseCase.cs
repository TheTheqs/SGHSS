// Application/UseCases/Administrators/Update/MakeBedAsAvailableUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Administrators.Update
{
    /// <summary>
    /// Caso de uso responsável por tornar uma cama disponível novamente,
    /// removendo o estado de manutenção quando aplicável.
    /// </summary>
    /// <remarks>
    /// Este caso de uso só permite a transição de estado a partir de 
    /// <see cref="BedStatus.UnderMaintenance"/> para <see cref="BedStatus.Available"/>.
    /// Caso a cama esteja em qualquer outro estado (ocupada, reservada, já disponível, etc.),
    /// uma exceção será lançada para evitar inconsistências na lógica operacional.
    /// </remarks>
    public class MakeBedAsAvailableUseCase
    {
        private readonly IBedRepository _bedRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso para definir uma cama como disponível.
        /// </summary>
        /// <param name="bedRepository">
        /// Repositório de camas utilizado para recuperar e atualizar a entidade.
        /// </param>
        public MakeBedAsAvailableUseCase(IBedRepository bedRepository)
        {
            _bedRepository = bedRepository;
        }

        /// <summary>
        /// Define a cama especificada como disponível, caso ela esteja em manutenção.
        /// </summary>
        /// <param name="bedId">O identificador único da cama a ser modificada.</param>
        /// <returns>Uma tarefa assíncrona representando a operação.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Lançada quando nenhuma cama com o ID informado é encontrada.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a cama não está em estado de manutenção.
        /// </exception>
        public async Task Handle(Guid bedId)
        {
            // Buscar cama no repositório
            Bed? bed = await _bedRepository.GetByIdAsync(bedId);

            if (bed is null)
                throw new KeyNotFoundException("Nenhuma cama foi encontrada com o ID informado.");

            // Apenas camas em manutenção podem voltar a ficar disponíveis
            if (bed.Status != BedStatus.UnderMaintenance)
                throw new InvalidOperationException("A cama só pode ser marcada como disponível se estiver em manutenção.");

            // Alterar estado para Available
            bed.Status = BedStatus.Available;

            // Persistir atualização
            await _bedRepository.UpdateAsync(bed);
        }
    }
}
