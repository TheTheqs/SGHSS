// Infra/Configurations/AdministratorConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para o tipo de entidade Administrator.
    /// </summary>
    /// <remarks>
    /// Esta configuração define o mapeamento de propriedades e os relacionamentos da entidade
    /// Administrator ao utilizar o model builder do Entity Framework Core.  
    /// Normalmente é aplicada dentro do método OnModelCreating de um DbContext para configurar
    /// o comportamento específico dessa entidade.
    /// </remarks>
    public class AdministratorConfiguration : IEntityTypeConfiguration<Administrator>
    {
        public void Configure(EntityTypeBuilder<Administrator> builder)
        {            

            // Propriedades específicas
            builder.Property(a => a.AccessLevel)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // Relacionamentos

            builder.HasMany(a => a.InventoryMovements)
                   .WithOne(im => im.Administrator);

            builder.HasMany(a => a.AuditReports)
                   .WithOne(r => r.CreatedBy);

            builder.HasMany(a => a.FinancialReports)
                   .WithOne(r => r.Administrator);
        }
    }
}
