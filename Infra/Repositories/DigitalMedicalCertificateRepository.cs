// Infra/Repositories/DigitalMedicalCertificateRepository

using Microsoft.EntityFrameworkCore;
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

        /// <summary>
        /// Retorna todos os atestados médicos digitais emitidos para o paciente especificado,
        /// utilizando consulta sem rastreamento para maior desempenho em operações de leitura.
        /// </summary>
        /// <remarks>
        /// A consulta aplica um filtro pelo identificador do paciente associado
        /// e retorna os resultados ordenados de forma decrescente pela data de criação
        /// (<see cref="DigitalMedicalCertificate.CreatedAt"/>), permitindo que camadas
        /// superiores exibam os registros mais recentes primeiro.
        /// 
        /// Caso nenhum atestado seja encontrado, o método retorna uma lista vazia.
        /// </remarks>
        /// <param name="patientId">Identificador do paciente cujos atestados devem ser retornados.</param>
        /// <returns>
        /// Uma lista somente leitura contendo os atestados digitais associados ao paciente.
        /// </returns>
        public async Task<IReadOnlyList<DigitalMedicalCertificate>> GetByPatientIdAsync(Guid patientId)
        {
            return await _context
                .DigitalMedicalCertificates
                .AsNoTracking()
                .Where(c => c.Patient != null && c.Patient.Id == patientId)
                .OrderByDescending(c => c.IssuedAt)
                .ToListAsync();
        }
    }
}
