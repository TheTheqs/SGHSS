// Program.cs for .NET 10 with Swagger/OpenAPI setup

using Microsoft.EntityFrameworkCore;
using SGHSS.Infra.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do DbContext com PostgreSQL
builder.Services.AddDbContext<SGHSSDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); // Configuração para compatibilidade com timestamps do PostgreSQL

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { } // Para integração de testes