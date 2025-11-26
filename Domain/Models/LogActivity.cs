//Domain/Models/LogActivity.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa um registro de atividade ou evento realizado por um usuário, para fins de registro e auditoria.
    /// </summary>
    /// <remarks>Uma instância de LogActivity captura detalhes sobre uma ação específica executada por um usuário,
    /// incluindo o horário, o tipo de ação, o resultado e as entidades relacionadas. Esta class é normalmente usada
    /// para rastrear operações de usuários em cenários de segurança, conformidade ou solução de problemas.</remarks>

    public class LogActivity
    {
        public Guid Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IpAddress IpAddress { get; set; } // Value Object
        public LogResult Result { get; set; } // Enum

        // Relacionamentos
        public User User { get; set; } = null!;
        public HealthUnit? HealthUnit { get; set; }

        // Construtor padrão
        public LogActivity() { }

        // ToString visando documentação de auditoria
        public override string ToString()
        {
            var lines = new List<string>
            {
                "==== LOG ACTIVITY REPORT ====",
                $"ID:               {Id}",
                $"Timestamp:        {Timestamp:yyyy-MM-dd HH:mm:ss zzz}",
                $"User:             {User?.Id} ({User?.Email})",
                $"Action:           {Action}",
                $"Result:           {Result}",
                $"Description:      {Description}",
                $"IP Address:       {IpAddress}",
                $"Health Unit:      {(HealthUnit is null ? "N/A" : $"{HealthUnit.Id} - {HealthUnit.Name}")}",
                "=============================="
            };

            return string.Join(Environment.NewLine, lines);
        }
    }
}
