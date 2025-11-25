// Infra/Repositories/ProfessionalRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Implementação do repositório responsável pelo acesso e manipulação
    /// dos dados de profissionais no banco, utilizando Entity Framework Core.
    /// </summary>
    public class ProfessionalRepository : IProfessionalRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Inicializa uma nova instância do repositório de profissionais.
        /// </summary>
        /// <param name="context">
        /// Contexto do Entity Framework responsável pela conexão e operações no banco.
        /// </param>
        public ProfessionalRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adiciona um novo profissional ao banco de dados de forma assíncrona.
        /// </summary>
        /// <param name="professional">
        /// Entidade <see cref="Professional"/> já validada pela camada de domínio.
        /// </param>
        /// <remarks>
        /// Este método executa o <c>AddAsync</c> seguido de um <c>SaveChangesAsync</c>,
        /// garantindo que o registro seja persistido imediatamente.
        /// </remarks>
        public async Task AddAsync(Domain.Models.Professional professional)
        {
            await _context.Professionals.AddAsync(professional);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Verifica se já existe um profissional cadastrado com o e-mail informado.
        /// </summary>
        /// <param name="email">
        /// Value Object <see cref="Domain.ValueObjects.Email"/> representando o e-mail normalizado.
        /// </param>
        /// <returns>
        /// <c>true</c> caso exista algum profissional com o e-mail informado;
        /// caso contrário, <c>false</c>.
        /// </returns>
        public async Task<bool> ExistsByEmailAsync(Domain.ValueObjects.Email email)
        {
            return await _context.Professionals
                .AnyAsync(p => p.Email == email);
        }

        /// <summary>
        /// Verifica se já existe um profissional cadastrado com a licença profissional informada.
        /// </summary>
        /// <param name="license">
        /// Value Object <see cref="Domain.ValueObjects.ProfessionalLicense"/> representando
        /// a licença normalizada.
        /// </param>
        /// <returns>
        /// <c>true</c> se algum profissional estiver registrado com a mesma licença;
        /// caso contrário, <c>false</c>.
        /// </returns>
        public async Task<bool> ExistsByProfessionalLicenseAsync(Domain.ValueObjects.ProfessionalLicense license)
        {
            return await _context.Professionals
                .AnyAsync(p => p.License == license);
        }
        /// <summary>
        /// Busca um profissional pelo seu identificador único.
        /// </summary>
        /// <param name="professionalId">ID do profissional desejado.</param>
        /// <returns>
        /// A entidade <see cref="Professional"/> localizada, ou <c>null</c>
        /// caso não exista registro com o ID informado.
        /// </returns>
        public async Task<Professional?> GetByIdAsync(Guid professionalId)
        {
            return await _context.Professionals
                .FirstOrDefaultAsync(p => p.Id == professionalId);
        }
    }
}
