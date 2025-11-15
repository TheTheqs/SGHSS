// Infra/Configurations/NotificationConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;

namespace SGHSS.Infra.Configurations
{
    /// <summary>
    /// Provides the Entity Framework Core configuration for the Notification entity.
    /// </summary>
    /// <remarks>
    /// Defines mappings for enums, text fields, timestamps, and the relationship
    /// with the User entity representing the notification recipient.
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
