// Infra/Repositories/BedRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável pelas operações de persistência relacionadas às camas (Beds).
    /// </summary>
    public class BedRepository : IBedRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de camas.
        /// </summary>
        /// <param name="context">O contexto de banco de dados utilizado para acesso aos dados.</param>
        public BedRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Atualiza os dados de uma cama existente no repositório.
        /// </summary>
        /// <param name="bed">
        /// A entidade <see cref="Bed"/> contendo os dados atualizados.
        /// </param>
        /// <returns>
        /// Uma tarefa assíncrona que representa a operação de atualização.
        /// </returns>
        /// <remarks>
        /// Este método sobrescreve os valores atuais da cama no banco de dados
        /// com os novos dados fornecidos. A validação de regras de negócio deve
        /// ser feita previamente pela camada de aplicação.
        /// </remarks>
        public async Task UpdateAsync(Bed bed)
        {
            _context.Beds.Update(bed);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Recupera uma cama pelo seu identificador único.
        /// </summary>
        /// <param name="bedId">O identificador da cama.</param>
        /// <returns>
        /// A entidade <see cref="Bed"/> correspondente ao identificador informado,
        /// ou <c>null</c> caso não seja encontrada.
        /// </returns>
        /// <remarks>
        /// Este método deve ser usado para recuperar o estado atual de uma cama 
        /// antes de realizar operações de atualização, verificação de disponibilidade
        /// ou qualquer outra regra de negócio relacionada ao seu status.
        /// </remarks>
        public async Task<Bed?> GetByIdAsync(Guid bedId)
        {
            return await _context.Beds
                .FirstOrDefaultAsync(b => b.Id == bedId);
        }
    }
}
