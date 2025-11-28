// Infra/Repositories/AdministratorRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável pelas operações de persistência relacionadas aos
    /// administradores do sistema.
    /// </summary>
    public class AdministratorRepository : IAdministratorRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de administradores.
        /// </summary>
        /// <param name="context">O contexto de banco de dados utilizado para acesso aos dados.</param>
        public AdministratorRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Determina de forma assíncrona se existe um administrador cadastrado
        /// com o e-mail especificado.
        /// </summary>
        /// <param name="email">Endereço de e-mail encapsulado em um Value Object.</param>
        /// <returns>
        /// <c>true</c> se existir um administrador com o e-mail informado; caso contrário, <c>false</c>.
        /// </returns>
        public async Task<bool> ExistsByEmailAsync(Email email)
        {
            return await _context.Administrators
                .AnyAsync(a => a.Email == email);
        }

        /// <summary>
        /// Recupera um administrador pelo seu identificador único.
        /// </summary>
        /// <param name="administratorId">O identificador do administrador.</param>
        /// <returns>
        /// A entidade <see cref="Administrator"/> correspondente ao identificador informado,
        /// ou <c>null</c> caso não seja encontrada.
        /// </returns>
        public async Task<Administrator?> GetByIdAsync(Guid administratorId)
        {
            return await _context.Administrators
                .FirstOrDefaultAsync(a => a.Id == administratorId);
        }

        /// <summary>
        /// Adiciona um novo administrador ao banco de dados.
        /// </summary>
        /// <param name="administrator">A entidade <see cref="Administrator"/> que será persistida.</param>
        /// <remarks>
        /// Esta operação utiliza o rastreamento de alterações do Entity Framework Core,
        /// permitindo incluir o administrador e suas entidades agregadas em uma única transação.
        /// </remarks>
        public async Task AddAsync(Administrator administrator)
        {
            await _context.Administrators.AddAsync(administrator);
            await _context.SaveChangesAsync();
        }

        
        /// <summary>
        /// Recupera todos os administradores cadastrados na base de dados.
        /// </summary>
        /// <returns>
        /// Uma lista somente leitura contendo todas as instâncias de <see cref="Administrator"/>.
        /// </returns>
        public async Task<IReadOnlyList<Administrator>> GetAllAsync()
        {
            return await _context.Administrators
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
