// Domain/Models/SchedulePolicy.cs

using TimeZone = SGHSS.Domain.ValueObjects.TimeZone; // Evita conflito com System.TimeZone

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa a política de agendamento de um profissional, incluindo a duração das consultas,
    /// fuso horário utilizado e as janelas semanais que definem sua disponibilidade.
    /// </summary>
    /// <remarks>
    /// Uma SchedulePolicy estabelece as regras que determinam como a agenda de um profissional
    /// é estruturada, especificando a duração padrão dos atendimentos, o fuso horário aplicado
    /// e as janelas de tempo recorrentes nas quais o profissional está ativo ou disponível.
    /// Também mantém o vínculo com a agenda do profissional e consolida as WeeklyWindows
    /// que compõem sua configuração de horários.
    /// </remarks>
    public class SchedulePolicy
    {
        public Guid Id { get; set; }
        public int DurationInMinutes { get; set; }
        public TimeZone TimeZone { get; set; }

        // Relacionamentos
        public ProfessionalSchedule ProfessionalSchedule { get; set; } = null!;
        public ICollection<WeeklyWindow> WeeklyWindows { get; set; } = new List<WeeklyWindow>();

        // Construtor padrão
        public SchedulePolicy() { }
    }
}
