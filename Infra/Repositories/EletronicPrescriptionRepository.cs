// Infra/Repositories/DigitalMedicalCertificateRepository


using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável pela persistência de prescrições eletrônicas
    /// no contexto do SGHSS.
    /// </summary>
    /// <remarks>
    /// Este repositório oferece operações para inclusão de novas
    /// <see cref="EletronicPrescription"/>, delegando ao Entity Framework Core
    /// o controle de transações e rastreamento de entidades.
    /// </remarks>
    public class EletronicPrescriptionRepository : IEletronicPrescriptionRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de prescrições eletrônicas.
        /// </summary>
        /// <param name="context">O contexto de banco de dados utilizado pelo EF Core.</param>
        public EletronicPrescriptionRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adiciona uma nova prescrição eletrônica ao banco de dados.
        /// </summary>
        /// <param name="prescription">
        /// Instância de <see cref="EletronicPrescription"/> a ser persistida.
        /// </param>
        public async Task AddAsync(EletronicPrescription prescription)
        {
            await _context.EletronicPrescriptions.AddAsync(prescription);
            await _context.SaveChangesAsync();
        }
    }
}
