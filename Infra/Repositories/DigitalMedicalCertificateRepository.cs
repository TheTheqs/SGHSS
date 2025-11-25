// Infra/Repositories/DigitalMedicalCertificateRepository

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável pela persistência de atestados médicos digitais
    /// no contexto do SGHSS.
    /// </summary>
    /// <remarks>
    /// Este repositório oferece operações para inclusão de novos
    /// <see cref="DigitalMedicalCertificate"/>, delegando ao Entity Framework Core
    /// o controle de transações e rastreamento de entidades.
    /// </remarks>
    public class DigitalMedicalCertificateRepository : IDigitalMedicalCertificateRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de atestados médicos digitais.
        /// </summary>
        /// <param name="context">O contexto de banco de dados utilizado pelo EF Core.</param>
        public DigitalMedicalCertificateRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adiciona um novo atestado médico digital ao banco de dados.
        /// </summary>
        /// <param name="certificate">
        /// Instância de <see cref="DigitalMedicalCertificate"/> a ser persistida.
        /// </param>
        public async Task AddAsync(DigitalMedicalCertificate certificate)
        {
            await _context.DigitalMedicalCertificates.AddAsync(certificate);
            await _context.SaveChangesAsync();
        }
    }
}
