// Program.cs – SGHSS API (.NET 10)
// Configuração inicial para testes verticais de autenticação (JWT)
// e criação automática de um Administrador SUPER padrão.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.Interfaces.Services;
using SGHSS.Application.UseCases.Administrators.Initialize;
using SGHSS.Application.UseCases.Authentication;
using SGHSS.Infra.Persistence;
using SGHSS.Infra.Repositories;
using SGHSS.Infra.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =====================
//  MVC + SWAGGER
// =====================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddScoped<IUserRepository, UserRepository>();

// UseCases
builder.Services.AddScoped<EnsureDefaultSuperAdministratorUseCase>();
builder.Services.AddScoped<AuthenticateUserUseCase>();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Ordem correta: autenticação -> autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Necessário para testes de integração
public partial class Program { }
