// Infra/Configurations/UserConfiguration.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Infra.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
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
                        email => email.Value,          // VO -> string
                        value => new Email(value))     // string -> VO
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
                   .WithOne(c => c.User)
                   .HasForeignKey(c => c.Id);

            builder.HasMany(u => u.LogActivities)
                   .WithOne(l => l.User)
                   .HasForeignKey(l => l.Id);

            builder.HasMany(u => u.Notifications)
                   .WithOne(n => n.Recipient)
                   .HasForeignKey(n => n.Id);

            // Herança TPH (um único table Users com Discriminator)
            builder
                .HasDiscriminator<string>("UserType")
                .HasValue<Patient>("Patient")
                .HasValue<Professional>("Professional")
                .HasValue<Administrator>("Administrator");
        }
    }
}
