// Infra/Repositories/UserRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável por realizar operações de leitura relacionadas à entidade
    /// <see cref="User"/> utilizando o Entity Framework Core.
    /// </summary>
    /// <remarks>
    /// Como o tipo <see cref="User"/> é abstrato e não possui uma tabela própria,
    /// esta implementação pesquisa o identificador informado nas tabelas concretas
    /// de usuários (pacientes, profissionais e administradores), retornando o primeiro
    /// resultado encontrado.
    /// </remarks>
    public class UserRepository : IUserRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de usuários.
        /// </summary>
        /// <param name="context">O contexto de banco de dados utilizado para acesso aos dados.</param>
        public UserRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Recupera um usuário pelo seu identificador único, buscando entre
        /// pacientes, profissionais e administradores.
        /// </summary>
        /// <param name="userId">Identificador do usuário a ser localizado.</param>
        /// <returns>
        /// Uma instância concreta de <see cref="User"/> (por exemplo,
        /// <see cref="Patient"/>, <see cref="Professional"/> ou <see cref="Administrator"/>),
        /// ou <c>null</c> caso nenhum usuário seja encontrado.
        /// </returns>
        public async Task<User?> GetByIdAsync(Guid userId)
        {
            // Tenta localizar como paciente
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (patient is not null)
            {
                return patient;
            }

            // Tenta localizar como profissional
            var professional = await _context.Professionals
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (professional is not null)
            {
                return professional;
            }

            // Tenta localizar como administrador
            var administrator = await _context.Administrators
                .FirstOrDefaultAsync(a => a.Id == userId);

            if (administrator is not null)
            {
                return administrator;
            }

            return null;
        }
    }
}
