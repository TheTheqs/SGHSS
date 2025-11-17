using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGHSS.Infra.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HealthUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserType = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    AccessLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    BirthDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Sex = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    License = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Specialty = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Availability = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false),
                    HealthUnitId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryItems_HealthUnits_HealthUnitId",
                        column: x => x.HealthUnitId,
                        principalTable: "HealthUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Link = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthUnitId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_HealthUnits_HealthUnitId",
                        column: x => x.HealthUnitId,
                        principalTable: "HealthUnits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appointments_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReportDetails = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditReports_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TermVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ConsentDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevocationDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TermHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalExpenses = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AdministratorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialReports_Users_AdministratorId",
                        column: x => x.AdministratorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthUnitPatient",
                columns: table => new
                {
                    HealthUnitsId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthUnitPatient", x => new { x.HealthUnitsId, x.PatientsId });
                    table.ForeignKey(
                        name: "FK_HealthUnitPatient_HealthUnits_HealthUnitsId",
                        column: x => x.HealthUnitsId,
                        principalTable: "HealthUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthUnitPatient_Users_PatientsId",
                        column: x => x.PatientsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthUnitProfessional",
                columns: table => new
                {
                    HealthUnitsId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthUnitProfessional", x => new { x.HealthUnitsId, x.ProfessionalsId });
                    table.ForeignKey(
                        name: "FK_HealthUnitProfessional_HealthUnits_HealthUnitsId",
                        column: x => x.HealthUnitsId,
                        principalTable: "HealthUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HealthUnitProfessional_Users_ProfessionalsId",
                        column: x => x.ProfessionalsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomeCares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthUnitId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeCares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeCares_HealthUnits_HealthUnitId",
                        column: x => x.HealthUnitId,
                        principalTable: "HealthUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeCares_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HomeCares_Users_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hospitalizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdmissionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DischargeDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hospitalizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hospitalizations_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    Result = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthUnitId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogActivities_HealthUnits_HealthUnitId",
                        column: x => x.HealthUnitId,
                        principalTable: "HealthUnits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LogActivities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRecords_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionalSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionalSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfessionalSchedules_Users_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    MovementType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InventoryItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdministratorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Users_AdministratorId",
                        column: x => x.AdministratorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DigitalMedicalCertificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Recommendations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IcpSignature = table.Column<string>(type: "text", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthUnitId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalMedicalCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalMedicalCertificates_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DigitalMedicalCertificates_HealthUnits_HealthUnitId",
                        column: x => x.HealthUnitId,
                        principalTable: "HealthUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DigitalMedicalCertificates_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DigitalMedicalCertificates_Users_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EletronicPrescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ValidUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Instructions = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IcpSignature = table.Column<string>(type: "text", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthUnitId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EletronicPrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EletronicPrescriptions_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EletronicPrescriptions_HealthUnits_HealthUnitId",
                        column: x => x.HealthUnitId,
                        principalTable: "HealthUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EletronicPrescriptions_Users_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EletronicPrescriptions_Users_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Beds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BedNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    HealthUnitId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Beds_HealthUnits_HealthUnitId",
                        column: x => x.HealthUnitId,
                        principalTable: "HealthUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Beds_Hospitalizations_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Hospitalizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MedicalRecordUpdates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MedicalRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfessionalId = table.Column<Guid>(type: "uuid", nullable: false),
                    HealthUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecordUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalRecordUpdates_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MedicalRecordUpdates_HealthUnits_HealthUnitId",
                        column: x => x.HealthUnitId,
                        principalTable: "HealthUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalRecordUpdates_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalRecordUpdates_Users_ProfessionalId",
                        column: x => x.ProfessionalId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchedulePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DurationInMinutes = table.Column<int>(type: "integer", nullable: false),
                    TimeZone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProfessionalScheduleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchedulePolicies_ProfessionalSchedules_ProfessionalSchedule~",
                        column: x => x.ProfessionalScheduleId,
                        principalTable: "ProfessionalSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDateTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProfessionalScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleSlots_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ScheduleSlots_ProfessionalSchedules_ProfessionalScheduleId",
                        column: x => x.ProfessionalScheduleId,
                        principalTable: "ProfessionalSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyWindows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    SchedulePolicyId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyWindows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyWindows_SchedulePolicies_SchedulePolicyId",
                        column: x => x.SchedulePolicyId,
                        principalTable: "SchedulePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_HealthUnitId",
                table: "Appointments",
                column: "HealthUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditReports_CreatedById",
                table: "AuditReports",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Beds_AppointmentId",
                table: "Beds",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Beds_HealthUnitId",
                table: "Beds",
                column: "HealthUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Consents_UserId",
                table: "Consents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalMedicalCertificates_AppointmentId",
                table: "DigitalMedicalCertificates",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DigitalMedicalCertificates_HealthUnitId",
                table: "DigitalMedicalCertificates",
                column: "HealthUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalMedicalCertificates_PatientId",
                table: "DigitalMedicalCertificates",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DigitalMedicalCertificates_ProfessionalId",
                table: "DigitalMedicalCertificates",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_EletronicPrescriptions_AppointmentId",
                table: "EletronicPrescriptions",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EletronicPrescriptions_HealthUnitId",
                table: "EletronicPrescriptions",
                column: "HealthUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_EletronicPrescriptions_PatientId",
                table: "EletronicPrescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_EletronicPrescriptions_ProfessionalId",
                table: "EletronicPrescriptions",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialReports_AdministratorId",
                table: "FinancialReports",
                column: "AdministratorId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthUnitPatient_PatientsId",
                table: "HealthUnitPatient",
                column: "PatientsId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthUnitProfessional_ProfessionalsId",
                table: "HealthUnitProfessional",
                column: "ProfessionalsId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeCares_HealthUnitId",
                table: "HomeCares",
                column: "HealthUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeCares_PatientId",
                table: "HomeCares",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeCares_ProfessionalId",
                table: "HomeCares",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Hospitalizations_PatientId",
                table: "Hospitalizations",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_HealthUnitId",
                table: "InventoryItems",
                column: "HealthUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_AdministratorId",
                table: "InventoryMovements",
                column: "AdministratorId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_InventoryItemId",
                table: "InventoryMovements",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LogActivities_HealthUnitId",
                table: "LogActivities",
                column: "HealthUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LogActivities_UserId",
                table: "LogActivities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientId",
                table: "MedicalRecords",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordUpdates_AppointmentId",
                table: "MedicalRecordUpdates",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordUpdates_HealthUnitId",
                table: "MedicalRecordUpdates",
                column: "HealthUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordUpdates_MedicalRecordId",
                table: "MedicalRecordUpdates",
                column: "MedicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecordUpdates_ProfessionalId",
                table: "MedicalRecordUpdates",
                column: "ProfessionalId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalSchedules_ProfessionalId",
                table: "ProfessionalSchedules",
                column: "ProfessionalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePolicies_ProfessionalScheduleId",
                table: "SchedulePolicies",
                column: "ProfessionalScheduleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_AppointmentId",
                table: "ScheduleSlots",
                column: "AppointmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleSlots_ProfessionalScheduleId",
                table: "ScheduleSlots",
                column: "ProfessionalScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyWindows_SchedulePolicyId",
                table: "WeeklyWindows",
                column: "SchedulePolicyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditReports");

            migrationBuilder.DropTable(
                name: "Beds");

            migrationBuilder.DropTable(
                name: "Consents");

            migrationBuilder.DropTable(
                name: "DigitalMedicalCertificates");

            migrationBuilder.DropTable(
                name: "EletronicPrescriptions");

            migrationBuilder.DropTable(
                name: "FinancialReports");

            migrationBuilder.DropTable(
                name: "HealthUnitPatient");

            migrationBuilder.DropTable(
                name: "HealthUnitProfessional");

            migrationBuilder.DropTable(
                name: "HomeCares");

            migrationBuilder.DropTable(
                name: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "LogActivities");

            migrationBuilder.DropTable(
                name: "MedicalRecordUpdates");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ScheduleSlots");

            migrationBuilder.DropTable(
                name: "WeeklyWindows");

            migrationBuilder.DropTable(
                name: "Hospitalizations");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "SchedulePolicies");

            migrationBuilder.DropTable(
                name: "HealthUnits");

            migrationBuilder.DropTable(
                name: "ProfessionalSchedules");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
