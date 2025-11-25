// Application/UseCases/Common/ScheduleSlotDto

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa um slot de agenda para uso na camada de aplicação.
    /// </summary>
    public class ScheduleSlotDto
    {
        /// <summary>
        /// Data e hora de início do slot.
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Data e hora de término do slot.
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Status lógico do slot no contexto da aplicação
        /// (ex.: disponível, reservado, bloqueado).
        /// </summary>
        public ScheduleSlotStatus Status { get; set; }
    }
}
