// Domain/Models/ProfessionalSchedule.cs

namespace SGHSS.Domain.Models
{
    /// <summary>
    /// Representa a agenda profissional de um prestador de saúde, incluindo a política de horário
    /// aplicada e os slots de atendimento disponíveis.
    /// </summary>
    /// <remarks>
    /// Esta classe associa um profissional às regras de agendamento que definem sua disponibilidade,
    /// além de manter a coleção de slots individuais que compõem sua agenda. Utilize este tipo para
    /// organizar, consultar e gerenciar a estrutura de horários de um profissional dentro do sistema.
    /// </remarks>
    public class ProfessionalSchedule
    {
        public Guid Id { get; set; }
        
        // Relacionamentos
        public Professional Professional { get; set; } = null!;
        public SchedulePolicy SchedulePolicy { get; set; } = null!;
        public ICollection<ScheduleSlot> ScheduleSlots { get; set; } = new List<ScheduleSlot>();

        // Construtor padrão
        public ProfessionalSchedule() { }
    }
}
