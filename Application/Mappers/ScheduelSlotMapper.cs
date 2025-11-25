using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Models;

namespace SGHSS.Application.Mappers
{
    /// <summary>
    /// Métodos de extensão para conversão de ScheduleSlotDto em ScheduleSlot de domínio.
    /// </summary>
    public static class ScheduleSlotMapper
    {
        /// <summary>
        /// Converte um ScheduleSlotDto em um ScheduleSlot de domínio,
        /// associando-o à agenda profissional informada.
        /// </summary>
        /// <param name="dto">Dados do slot na camada de aplicação.</param>
        /// <param name="professionalSchedule">Agenda profissional à qual o slot pertence.</param>
        /// <returns>Instância de ScheduleSlot pronta para uso na camada de domínio.</returns>
        public static ScheduleSlot ToDomain(
            this ScheduleSlotDto dto,
            ProfessionalSchedule professionalSchedule)
        {
            return new ScheduleSlot
            {
                StartDateTime = dto.StartDateTime,
                EndDateTime = dto.EndDateTime,
                Status = dto.Status,
                ProfessionalSchedule = professionalSchedule,
                Appointment = null
            };
        }
    }
}
