// Application/UseCases/ProfessionalSchedules/Consult/ConsultReservedScheduleSlotsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.ProfessionalSchedules.Consult
{
    /// <summary>
    /// Caso de uso responsável por consultar os horários reservados
    /// de uma agenda profissional específica.
    /// </summary>
    /// <remarks>
    /// Este caso de uso encapsula a lógica de carregamento da agenda
    /// profissional e de seus slots associados, filtrando apenas aqueles
    /// que se encontram em estado reservado e projetando-os para DTOs
    /// apropriados para a camada de interface.
    /// </remarks>
    public class ConsultReservedScheduleSlotsUseCase
    {
        private readonly IProfessionalScheduleRepository _professionalScheduleRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de consulta de horários reservados
        /// da agenda profissional.
        /// </summary>
        /// <param name="professionalScheduleRepository">
        /// Repositório responsável por acessar os dados de agendas profissionais
        /// na camada de infraestrutura.
        /// </param>
        public ConsultReservedScheduleSlotsUseCase(
            IProfessionalScheduleRepository professionalScheduleRepository)
        {
            _professionalScheduleRepository = professionalScheduleRepository;
        }

        /// <summary>
        /// Executa o fluxo de consulta dos horários reservados para a agenda informada.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o identificador da agenda profissional cujos horários
        /// reservados serão consultados.
        /// </param>
        /// <returns>
        /// Um <see cref="ConsultReservedScheduleSlotResponse"/> contendo o identificador
        /// da agenda e a coleção de slots atualmente reservados.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request é nulo.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Lançada quando o identificador da agenda profissional é vazio.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a agenda profissional não é encontrada.
        /// </exception>
        public async Task<ConsultReservedScheduleSlotResponse> Handle(
            ConsultReservedScheduleSlotsRequest request)
        {
            // Validação defensiva básica de entrada
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request), "A requisição não pode ser nula.");
            }

            if (request.ProfessionalScheduleId == Guid.Empty)
            {
                throw new ArgumentException("O identificador da agenda profissional não pode ser vazio.");
            }

            // Busca a agenda profissional com seus slots associados.
            var professionalSchedule = await _professionalScheduleRepository
                .GetByIdWithSlotsAsync(request.ProfessionalScheduleId);

            if (professionalSchedule is null)
            {
                throw new InvalidOperationException(
                    "Não foi possível localizar uma agenda profissional para o identificador informado."
                );
            }

            // Filtra apenas os slots em estado reservado.
            var reservedSlots = professionalSchedule.ScheduleSlots
                .Where(slot => slot.Status == ScheduleSlotStatus.Reserved)
                .OrderBy(slot => slot.StartDateTime)
                .Select(slot => new ScheduleSlotDto
                {
                    StartDateTime = slot.StartDateTime,
                    EndDateTime = slot.EndDateTime,
                    Status = slot.Status
                })
                .ToList();

            // Monta o response final.
            return new ConsultReservedScheduleSlotResponse
            {
                ProfessionalScheduleId = professionalSchedule.Id,
                ScheduleSlots = reservedSlots
            };
        }
    }
}
