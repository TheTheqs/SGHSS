// Infra/Persistence/SGHSSDbContext.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Persistence
{
    /// <summary>
    /// Representa o contexto de banco de dados do Entity Framework Core para a aplicação SGHSS,
    /// fornecendo acesso às entidades de domínio e operações de persistência.
    /// </summary>
    /// <remarks>
    /// Este contexto gerencia o modelo de dados da aplicação e é utilizado para consultar e salvar
    /// instâncias de entidades como pacientes, profissionais, agendamentos, prontuários, itens de estoque
    /// e outros objetos do domínio da saúde.  
    /// 
    /// Normalmente é configurado e injetado por meio de dependency injection.  
    /// O contexto também aplica automaticamente todas as configurações de entidades encontradas no assembly,
    /// permitindo um mapeamento modular e personalizável do modelo de dados.
    /// </remarks>

    public class SGHSSDbContext : DbContext
    {
        public SGHSSDbContext(DbContextOptions<SGHSSDbContext> options)
            : base(options)
        {
        }

        // DbSets da hierarquia de usuário
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Professional> Professionals { get; set; } = null!;
        public DbSet<Administrator> Administrators { get; set; } = null!;

        // Outros DbSets
        public DbSet<Consent> Consents { get; set; } = null!;
        public DbSet<LogActivity> LogActivities { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        public DbSet<HealthUnit> HealthUnits { get; set; } = null!;
        public DbSet<ProfessionalSchedule> ProfessionalSchedules { get; set; } = null!;
        public DbSet<SchedulePolicy> SchedulePolicies { get; set; } = null!;
        public DbSet<WeeklyWindow> WeeklyWindows { get; set; } = null!;
        public DbSet<ScheduleSlot> ScheduleSlots { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<MedicalRecord> MedicalRecords { get; set; } = null!;
        public DbSet<MedicalRecordUpdate> MedicalRecordUpdates { get; set; } = null!;
        public DbSet<EletronicPrescription> EletronicPrescriptions { get; set; } = null!;
        public DbSet<DigitalMedicalCertificate> DigitalMedicalCertificates { get; set; } = null!;
        public DbSet<Hospitalization> Hospitalizations { get; set; } = null!;
        public DbSet<HomeCare> HomeCares { get; set; } = null!;
        public DbSet<AuditReport> AuditReports { get; set; } = null!;
        public DbSet<FinancialReport> FinancialReports { get; set; } = null!;
        public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
        public DbSet<InventoryMovement> InventoryMovements { get; set; } = null!;
        public DbSet<Bed> Beds { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplica todas as IEntityTypeConfiguration do assembly de Infra
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SGHSSDbContext).Assembly);
        }
    }
}
