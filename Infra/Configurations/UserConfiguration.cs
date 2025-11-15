// Infra/Configurations/UserConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para o tipo de entidade User.
    /// </summary>
    /// <remarks>
    /// Esta configuração define o mapeamento de propriedades, conversões de Value Objects,
    /// relacionamentos e o mapeamento de herança para a entidade User.  
    /// Deve ser registrada no model builder para garantir que a entidade User seja
    /// corretamente mapeada para o esquema do banco de dados.
    /// </remarks>

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Tabela única da hierarquia
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            // Enum UserStatus como string
            builder.Property(u => u.Status)
                   .HasConversion<string>()
                   .HasMaxLength(50);

            // Email como VO
            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasConversion(
                        email => email.Value,          
                        value => new Email(value))
                   .HasMaxLength(254);

            // Phone como VO
            builder.Property(u => u.Phone)
                   .IsRequired()
                   .HasConversion(
                        phone => phone.Value,
                        value => new Phone(value))
                   .HasMaxLength(20);

            // Relacionamentos comuns
            builder.HasMany(u => u.Consents)
                   .WithOne(c => c.User);

            builder.HasMany(u => u.LogActivities)
                   .WithOne(l => l.User);

            builder.HasMany(u => u.Notifications)
                   .WithOne(n => n.Recipient);

            // Herança TPH (um único table Users com Discriminator)
            builder
                .HasDiscriminator<string>("UserType")
                .HasValue<Patient>("Patient")
                .HasValue<Professional>("Professional")
                .HasValue<Administrator>("Administrator");
        }
    }
}
