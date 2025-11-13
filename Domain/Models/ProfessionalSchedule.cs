// Domain/Models/ProfessionalSchedule.cs

namespace SGHSS.Domain.Models
{
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
