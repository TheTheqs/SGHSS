// Infra/Configurations/NotificationConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Fornece a configuração do Entity Framework Core para a entidade Notification.
    /// </summary>
    /// <remarks>
    /// Define o mapeamento de enums, campos textuais, timestamps
    /// e o relacionamento com a entidade User que representa o destinatário.
    /// </remarks>
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Message)
                   .IsRequired()
                   .HasMaxLength(2000);

            builder.Property(n => n.CreatedAt)
                   .IsRequired();

            builder.Property(n => n.Channel)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(n => n.Status)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            // Relacionamento

            builder.HasOne(n => n.Recipient)
                   .WithMany(u => u.Notifications);
        }
    }
}
