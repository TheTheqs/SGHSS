// Infra/Repositories/PatientRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    public class AdministratorRepository: IAdministratorRepository
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
        /// Determina de forma assíncrona se existe um paciente cadastrado com o e-mail especificado.
        /// </summary>
        /// <param name="email">Endereço de e-mail encapsulado em um Value Object.</param>
        /// <returns>
        /// <c>true</c> se existir um paciente com o e-mail informado; caso contrário, <c>false</c>.
        /// </returns>
        public async Task<bool> ExistsByEmailAsync(Email email)
        {
            return await _context.Administrators
                .AnyAsync(a => a.Email == email);
        }
        /// <summary>
        /// Adiciona um novo paciente à base de dados, incluindo todos os seus relacionamentos configurados
        /// (como consentimentos, endereços e dados pessoais).
        /// </summary>
        /// <param name="administrator">A entidade <see cref="Administrator"/> que será persistida.</param>
        /// <remarks>
        /// Esta operação utiliza o rastreamento de alterações do Entity Framework Core,
        /// permitindo incluir o paciente e suas entidades agregadas em uma única transação.
        /// </remarks>
        public async Task AddAsync(Administrator administrator)
        {
            await _context.Administrators.AddAsync(administrator);
            await _context.SaveChangesAsync();
        }
    }
}
