// Infra/Configurations/ConsentConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade Consent.
    /// </summary>
    /// <remarks>
    /// Esta configuração define o mapeamento de enums, value objects, propriedades simples
    /// e o relacionamento com a entidade User, utilizando chaves estrangeiras sombreadas.
    /// Também ignora propriedades calculadas que não devem ser persistidas no banco.
    /// </remarks>
    public class ConsentConfiguration : IEntityTypeConfiguration<Consent>
    {
        public void Configure(EntityTypeBuilder<Consent> builder)
        {
            builder.ToTable("Consents");

            builder.HasKey(c => c.Id);

            // Enums como string
            builder.Property(c => c.Scope)
                   .HasConversion<string>()
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(c => c.Channel)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(c => c.TermVersion)
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(c => c.ConsentDate)
                   .IsRequired();

            builder.Property(c => c.RevocationDate)
                   .IsRequired(false);

            // Ignorar propriedade calculada
            builder.Ignore(c => c.Status);

            // VALUE OBJECT: TermHash
            builder.Property(c => c.TermHash)
                   .IsRequired()
                   .HasConversion(
                        hash => hash.Value,          
                        value => new HashDigest(value)
                    )
                   .HasMaxLength(128);

            // Relacionamentos
            builder.HasOne(c => c.User)
                   .WithMany(u => u.Consents);
        }
    }
}
