// Infra/Persistence/SGHSSDbContext.cs

using Microsoft.EntityFrameworkCore;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Persistence
{
    /// <summary>
    /// Represents the Entity Framework Core database context for the SGHSS application, providing access to domain
    /// entities and database operations.
    /// </summary>
    /// <remarks>This context manages the application's data model and is used to query and save instances of
    /// entities such as patients, professionals, appointments, medical records, inventory items, and related healthcare
    /// domain objects. It is typically configured and injected via dependency injection. The context applies all entity
    /// configurations found in the assembly, enabling modular mapping and customization of the data model.</remarks>
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
