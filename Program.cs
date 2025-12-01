// Program.cs – SGHSS API (.NET 10)
// Configuração inicial para testes verticais de autenticação (JWT)
// e criação automática de um Administrador SUPER padrão.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.Interfaces.Services;
using SGHSS.Application.UseCases.Administrators.Initialize;
using SGHSS.Application.UseCases.AuditReports.Consult;
using SGHSS.Application.UseCases.AuditReports.Generate;
using SGHSS.Application.UseCases.Authentication;
using SGHSS.Application.UseCases.LogActivities.Register;
using SGHSS.Infra.Persistence;
using SGHSS.Infra.Repositories;
using SGHSS.Infra.Services;
using System.Text;
using SGHSS.Application.UseCases.Administrators.Register;
using SGHSS.Application.UseCases.Patients.Register;
using SGHSS.Application.UseCases.Professionals.Register;
using SGHSS.Application.UseCases.Administrators.Read;
using SGHSS.Application.UseCases.Patients.Read;
using SGHSS.Application.UseCases.Professionals.Read;
using SGHSS.Application.UseCases.Administrators.Update;
using SGHSS.Application.UseCases.ProfessionalSchedules.Consult;
using SGHSS.Application.UseCases.Patients.Update;
using SGHSS.Application.UseCases.DigitalMedicalCertificates.Issue;
using SGHSS.Application.UseCases.Appointments.Register;
using SGHSS.Application.UseCases.Appointments.Read;
using SGHSS.Application.UseCases.Appointments.GetLink;

var builder = WebApplication.CreateBuilder(args);

// =====================
//  MVC + SWAGGER
// =====================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minha API", Version = "v1" });

    // Config JWT Bearer (simples e direto)
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Exemplo: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// =====================
//  DB CONTEXT (PostgreSQL)
// =====================

builder.Services.AddDbContext<SGHSSDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Compatibilidade de timestamp com Npgsql
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// =====================
//  DEPENDÊNCIAS (DI)
// =====================

// Serviços
builder.Services.AddScoped<ITokenService, TokenService>();

// Repositórios
builder.Services.AddScoped<IAdministratorRepository, AdministratorRepository>();
builder.Services.AddScoped<IHealthUnitRepository,  HealthUnitRepository>();
builder.Services.AddScoped<ILogActivityRepository, LogActivityRepository>();
builder.Services.AddScoped<IAuditReportRepository, AuditReportRepository>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBedRepository, BedRepository>();
builder.Services.AddScoped<IHospitalizationRepository, HospitalizationRepository>();
builder.Services.AddScoped<IProfessionalScheduleRepository, ProfessionalScheduleRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IDigitalMedicalCertificateRepository, DigitalMedicalCertificateRepository>();

// UseCases
builder.Services.AddScoped<EnsureDefaultSuperAdministratorUseCase>();
builder.Services.AddScoped<AuthenticateUserUseCase>();
builder.Services.AddScoped<RegisterLogActivityUseCase>();
builder.Services.AddScoped<GenerateAuditReportUseCase>();
builder.Services.AddScoped<ConsultAuditReportsByAdministratorUseCase>();
builder.Services.AddScoped<RegisterAdministratorUseCase>();
builder.Services.AddScoped<RegisterPatientUseCase>();
builder.Services.AddScoped<RegisterProfessionalUseCase>();
builder.Services.AddScoped<RegisterHealthUnitUseCase>();
builder.Services.AddScoped<GetAllAdministratorsUseCase>();
builder.Services.AddScoped<GetAllPatientsUseCase>();
builder.Services.AddScoped<GetAllProfessionalsUseCase>();
builder.Services.AddScoped<GetAllHealthUnitsUseCase>();
builder.Services.AddScoped<ManageBedsUseCase>();
builder.Services.AddScoped<ConsultHealthUnitBedsUseCase>();
builder.Services.AddScoped<MakeBedAsAvailableUseCase>();
builder.Services.AddScoped<MakeBedAsUnderMaintenanceUseCase>();
builder.Services.AddScoped<HospitalizePatientUseCase>();
builder.Services.AddScoped<DischargePatientUseCase>();
builder.Services.AddScoped<GenerateAvailableSlotsUseCase>();
builder.Services.AddScoped<UpdateMedicalRecordUseCase>();
builder.Services.AddScoped<IssueDigitalMedicalCertificateUseCase>();
builder.Services.AddScoped<ScheduleAppointmentUseCase>();
builder.Services.AddScoped<GetPatientAppointmentsUseCase>();
builder.Services.AddScoped<GetAppointmentLinkUseCase>();


// =====================
//  AUTENTICAÇÃO JWT
// =====================

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtSecret = jwtSection["SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey não configurada no appsettings.json.");

var jwtIssuer = jwtSection["Issuer"] ?? "sghss-api";
var jwtAudience = jwtSection["Audience"] ?? "sghss-users";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

var app = builder.Build();

// =====================
//  SEED SUPER ADMIN
// =====================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var superAdminInitializer =
        services.GetRequiredService<EnsureDefaultSuperAdministratorUseCase>();

    // Vai verificar se já existe o e-mail padrão; se não existe, cria o SUPER admin.
    await superAdminInitializer.Handle();
}

// =====================
//  PIPELINE HTTP
// =====================

// Middlewares
app.UseMiddleware<SGHSS.Interface.Middlewares.GlobalExceptionHandlingMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minha API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Necessário para testes de integração
public partial class Program { }
