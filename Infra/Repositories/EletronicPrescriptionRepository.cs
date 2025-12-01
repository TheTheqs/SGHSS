// Infra/Repositories/DigitalMedicalCertificateRepository


using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// Retorna todas as prescrições eletrônicas emitidas para o paciente especificado,
        /// utilizando consulta sem rastreamento para maior desempenho.
        /// </summary>
        /// <remarks>
        /// A consulta ordena os resultados pela data de criação em ordem decrescente
        /// (<see cref="EletronicPrescription.CreatedAt"/>), permitindo que camadas
        /// superiores exibam primeiro as prescrições mais recentes.
        ///
        /// Caso nenhum registro exista para o paciente informado, uma lista vazia é retornada.
        /// </remarks>
        /// <param name="patientId">Identificador do paciente cujas prescrições devem ser retornadas.</param>
        /// <returns>
        /// Uma lista somente leitura contendo todas as prescrições associadas ao paciente.
        /// </returns>
        public async Task<IReadOnlyList<EletronicPrescription>> GetByPatientIdAsync(Guid patientId)
        {
            return await _context
                .EletronicPrescriptions
                .AsNoTracking()
                .Where(p => p.Patient != null && p.Patient.Id == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
