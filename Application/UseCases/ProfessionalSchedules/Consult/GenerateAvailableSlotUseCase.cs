// Application/UseCases/ProfessionalSchedules/Consult/GenerateAvailableSlotsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Domain.Services;

namespace SGHSS.Application.UseCases.ProfessionalSchedules.Consult
{
    /// <summary>
    /// Representa a requisição para geração/consulta de horários disponíveis
    /// na agenda de um profissional em um determinado intervalo de tempo.
    /// </summary>
    public class GenerateAvailableSlotsRequest
    {
        /// <summary>
        /// Identificador do profissional cuja agenda será consultada.
        /// </summary>
        public Guid ProfessionalId { get; set; }

        /// <summary>
        /// Data e hora inicial (opcional) do intervalo de consulta de disponibilidade.
        /// 
        /// Quando não informado, o sistema utilizará a data/hora atual como início.
        /// </summary>
        public DateTime? From { get; set; }

        /// <summary>
        /// Data e hora final (opcional) do intervalo de consulta de disponibilidade.
        /// 
        /// Quando não informado, o sistema utilizará um horizonte de dois meses
        /// a partir da data/hora inicial.
        /// </summary>
        public DateTime? To { get; set; }
    }

    /// <summary>
    /// Representa a resposta da consulta de horários disponíveis de um profissional.
    /// </summary>
    public class GenerateAvailableSlotsResponse
    {
        /// <summary>
        /// Identificador do profissional cuja agenda foi consultada.
        /// </summary>
        public Guid ProfessionalId { get; set; }

        /// <summary>
        /// Coleção de intervalos disponíveis para agendamento.
        /// </summary>
        public IReadOnlyCollection<ScheduleSlotDto> Slots { get; set; } = new List<ScheduleSlotDto>();
    }

    /// <summary>
    /// Caso de uso responsável por calcular e retornar os horários disponíveis
    /// na agenda de um profissional em um dado intervalo de tempo.
    /// </summary>
    /// <remarks>
    /// Este caso de uso carrega a agenda do profissional, incluindo a SchedulePolicy,
    /// suas WeeklyWindows e os ScheduleSlots já existentes, e utiliza o serviço de
    /// domínio <see cref="ScheduleAvailabilityService"/> para determinar os horários
    /// livres (ainda não ocupados) para agendamento.
    /// </remarks>
    public class GenerateAvailableSlotsUseCase
    {
        private readonly IProfessionalScheduleRepository _professionalScheduleRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso para geração de slots disponíveis.
        /// </summary>
        /// <param name="professionalScheduleRepository">
        /// Repositório responsável por acessar a agenda profissional e suas políticas.
        /// </param>
        public GenerateAvailableSlotsUseCase(IProfessionalScheduleRepository professionalScheduleRepository)
        {
            _professionalScheduleRepository = professionalScheduleRepository;
        }

        /// <summary>
        /// Executa a consulta de horários disponíveis para um profissional.
        /// </summary>
        /// <param name="request">Dados necessários para identificar o profissional e o intervalo.</param>
        /// <returns>
        /// Um objeto contendo o identificador do profissional e a lista de intervalos
        /// de horários disponíveis para agendamento.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando a requisição informada é nula.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando não há agenda configurada para o profissional informado.
        /// </exception>
        public async Task<GenerateAvailableSlotsResponse> Handle(GenerateAvailableSlotsRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Define o intervalo de consulta.
            DateTime from = request.From ?? DateTime.UtcNow;
            DateTime to = request.To ?? from.AddMonths(2);

            if (to <= from)
            {
                throw new InvalidOperationException("A data final do intervalo deve ser maior que a data inicial.");
            }

            // Carrega a agenda do profissional com as informações necessárias:
            // - SchedulePolicy
            // - WeeklyWindows
            // - ScheduleSlots dentro do período [from, to)
            ProfessionalSchedule? schedule = await _professionalScheduleRepository
                .GetByProfessionalIdWithPolicyAndSlotsAsync(request.ProfessionalId, from, to);

            if (schedule is null || schedule.SchedulePolicy is null)
            {
                throw new InvalidOperationException("Nenhuma agenda configurada foi encontrada para o profissional informado.");
            }

            // Garante que apenas os slots relevantes ao intervalo foram considerados.
            var existingSlots = schedule.ScheduleSlots
                .Where(slot => slot.StartDateTime < to && slot.EndDateTime > from)
                .ToList();

            // Calcula os intervalos disponíveis utilizando o serviço de domínio.
            var availableIntervals = ScheduleAvailabilityService.GenerateAvailableIntervals(
                schedule.SchedulePolicy,
                existingSlots,
                from,
                to);

            // Mapeia o resultado para DTOs de saída.
            var slotDtos = availableIntervals
                .Select(interval => new ScheduleSlotDto
                {
                    StartDateTime = interval.Start,
                    EndDateTime = interval.End,
                    Status = ScheduleSlotStatus.Available
                })
                .ToList();

            return new GenerateAvailableSlotsResponse
            {
                ProfessionalId = request.ProfessionalId,
                Slots = slotDtos
            };
        }
    }
}
