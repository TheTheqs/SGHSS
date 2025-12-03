// Infra/Repositories/PatientRepository.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;
using SGHSS.Infra.Persistence;

namespace SGHSS.Infra.Repositories
{
    /// <summary>
    /// Repositório responsável por realizar operações de persistência relacionadas à entidade
    /// <see cref="Patient"/> utilizando o Entity Framework Core.
    /// </summary>
    /// <remarks>
    /// Esta implementação segue o padrão de repositório e fornece uma abstração sobre o acesso ao banco,
    /// permitindo que a camada de aplicação trabalhe sem dependências diretas do EF Core.
    /// </remarks>
    public class PatientRepository : IPatientRepository
    {
        private readonly SGHSSDbContext _context;

        /// <summary>
        /// Cria uma nova instância do repositório de pacientes.
        /// </summary>
        /// <param name="context">O contexto de banco de dados utilizado para acesso aos dados.</param>
        public PatientRepository(SGHSSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Recupera um paciente seu identificador único,
        /// incluindo suas listas e relacionamentos relevantes
        /// </summary>
        /// <param name="patientId">Identificador do paciente.</param>
        /// <returns>
        /// A entidade <see cref="Patient"/> correspondente ao identificador,
        /// ou <c>null</c> caso não seja encontrada.
        /// </returns>
        /// <remarks>
        /// Inclui o carregamento explícito de seus relacionamentos.
        /// </remarks>
        public async Task<Patient?> GetByIdAsync(Guid patientId)
        {
            return await _context.Patients
                .Include(p => p.Hospitalizations)
                    .ThenInclude(h => h.Bed)
                .Include(p => p.MedicalRecord)
                    .ThenInclude(mr => mr.Updates)
                        .ThenInclude(u => u.Professional)
                .Include(p => p.MedicalRecord)
                    .ThenInclude(mr => mr.Updates)
                        .ThenInclude(u => u.HealthUnit)
                .Include(p => p.MedicalRecord)
                    .ThenInclude(mr => mr.Updates)
                        .ThenInclude(u => u.Appointment)
                .FirstOrDefaultAsync(p => p.Id == patientId);
        }

        /// <summary>
        /// Verifica de forma assíncrona se já existe um paciente cadastrado com o CPF informado.
        /// </summary>
        /// <param name="cpf">CPF do paciente encapsulado em um Value Object.</param>
        /// <returns>
        /// <c>true</c> se existir um paciente com o CPF informado; caso contrário, <c>false</c>.
        /// </returns>
        public async Task<bool> ExistsByCpfAsync(Cpf cpf)
        {
            return await _context.Patients
                .AnyAsync(p => p.Cpf == cpf);
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
            return await _context.Patients
                .AnyAsync(p => p.Email == email);
        }

        /// <summary>
        /// Atualiza os dados do paciente no banco de dados.
        /// </summary>
        /// <param name="patient">Entidade <see cref="Patient"/> que será atualizada.</param>
        /// <remarks>
        /// O Entity Framework rastreia a entidade e salva todas as mudanças
        /// realizadas, incluindo alterações em agregados relacionados.
        /// </remarks>
        public async Task UpdateAsync(Patient patient)
        {
          _context.Patients.Update(patient);
          await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adiciona um novo paciente à base de dados, incluindo todos os seus relacionamentos configurados
        /// (como consentimentos, endereços e dados pessoais).
        /// </summary>
        /// <param name="patient">A entidade <see cref="Patient"/> que será persistida.</param>
        /// <remarks>
        /// Esta operação utiliza o rastreamento de alterações do Entity Framework Core,
        /// permitindo incluir o paciente e suas entidades agregadas em uma única transação.
        /// </remarks>
        public async Task AddAsync(Patient patient)
        {
            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
        }
        
        /// <summary>
        /// Recupera todos os pacientes cadastrados na base de dados.
        /// </summary>
        /// <returns>
        /// Uma lista somente leitura contendo todas as instâncias de <see cref="Patient"/>.
        /// </returns>
        public async Task<IReadOnlyList<Patient>> GetAllAsync()
        {
            return await _context.Patients
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
