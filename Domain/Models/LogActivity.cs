//Domain/Models/LogActivity.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    public class LogActivity
    {
        public Guid Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IpAddress IpAddress { get; set; }
        public LogResult Result { get; set; }

        // Relacionamentos
        public User User { get; set; } = null!;
        public HealthUnit? HealthUnit { get; set; }

        // Construtor padrão
        public LogActivity() { }
    }
}
