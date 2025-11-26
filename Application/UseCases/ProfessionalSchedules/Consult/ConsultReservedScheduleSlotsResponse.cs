// Application/UseCases/ProfessionalSchedules/Consult/ConsultReservedScheduleSlotsRequest.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.ProfessionalSchedules.Consult
{
    /// <summary>
    /// Representa a resposta retornada ao consultar os horários reservados
    /// de uma agenda profissional específica.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado para transportar os dados resultantes do caso de uso
    /// <see cref="ConsultReservedScheduleSlotsUseCase"/>, contendo o identificador
    /// da agenda consultada e a coleção de horários reservados associados a ela.
    /// Ele abstrai a estrutura interna de entidades, entregando apenas as
    /// informações necessárias à camada superior.
    /// </remarks>
    public sealed class ConsultReservedScheduleSlotResponse
    {
        /// <summary>
        /// Identificador único da agenda profissional consultada.
        /// </summary>
        /// <remarks>
        /// Este valor reflete o <c>ProfessionalScheduleId</c> recebido na request,
        /// permitindo rastrear qual agenda teve seus horários reservados recuperados.
        /// </remarks>
        public Guid ProfessionalScheduleId { get; init; }

        /// <summary>
        /// Coleção de horários que estão atualmente reservados na agenda profissional.
        /// </summary>
        /// <remarks>
        /// Cada item desta coleção representa um slot de horário ocupado, mapeado
        /// através do DTO <see cref="ScheduleSlotDto"/>, garantindo que a camada 
        /// de apresentação receba apenas informações estruturadas e independentes
        /// da modelagem interna da entidade <c>ScheduleSlot</c>.
        /// </remarks>
        public ICollection<ScheduleSlotDto> ScheduleSlots { get; init; } = new List<ScheduleSlotDto>();
    }
}
