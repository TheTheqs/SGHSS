// Application/UseCases/Appointment/Register/ScheduleAppointmentUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.Mappers;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.UseCases.Appointments.Register
{
    /// <summary>
    /// Caso de uso responsável por agendar uma nova consulta
    /// a partir de um slot de agenda disponível.
    /// </summary>
    public class ScheduleAppointmentUseCase
    {
        private readonly IProfessionalScheduleRepository _professionalScheduleRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public ScheduleAppointmentUseCase(
            IProfessionalScheduleRepository professionalScheduleRepository,
            IPatientRepository patientRepository,
            IAppointmentRepository appointmentRepository)
        {
            _professionalScheduleRepository = professionalScheduleRepository;
            _patientRepository = patientRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<ScheduleAppointmentResponse> Handle(ScheduleAppointmentRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            if (request.Slot is null)
                throw new InvalidOperationException("É obrigatório informar o slot desejado para agendamento.");

            var requestedStart = request.Slot.StartDateTime;
            var requestedEnd = request.Slot.EndDateTime;

            if (requestedEnd <= requestedStart)
                throw new InvalidOperationException("O horário final deve ser maior que o horário inicial.");

            // 1) Carrega agenda + policy + slots do profissional
            ProfessionalSchedule? schedule = await _professionalScheduleRepository
                .GetByProfessionalIdWithPolicyAndSlotsAsync(request.ProfessionalId, requestedStart, requestedEnd);

            if (schedule is null || schedule.SchedulePolicy is null)
                throw new InvalidOperationException("Nenhuma agenda configurada foi encontrada para o profissional informado.");

            var policy = schedule.SchedulePolicy;

            // 2) Verifica se o slot desejado conflita com algum slot já existente
            bool hasConflict = schedule.ScheduleSlots.Any(slot =>
                slot.StartDateTime < requestedEnd &&
                requestedStart < slot.EndDateTime);

            if (hasConflict)
                throw new InvalidOperationException("Já existe um agendamento conflitante para o horário solicitado.");

            // 3) Verifica se o slot está dentro da política (WeeklyWindows + duração)
            bool fitsPolicy = SlotFitsPolicy(policy, requestedStart, requestedEnd);

            if (!fitsPolicy)
                throw new InvalidOperationException("O horário solicitado não está dentro da política de agendamento do profissional.");

            // 4) Carrega o paciente
            Patient? patient = await _patientRepository.GetByIdAsync(request.PatientId);
            if (patient is null)
                throw new InvalidOperationException("Paciente informado não foi encontrado.");

            // 5) Converte o DTO de slot para domínio e associa à agenda
            var slotDomain = request.Slot.ToDomain(schedule);

            // Aqui definimos explicitamente o status como ocupado/reservado.
            // Ajuste para o valor correto do seu enum, se necessário.
            slotDomain.Status = ScheduleSlotStatus.Reserved;

            schedule.ScheduleSlots.Add(slotDomain);

            // 6) Cria o Appointment com base no slot

            var appointment = new Appointment
            {
                StartTime = new DateTimeOffset(requestedStart, TimeSpan.Zero),
                EndTime = new DateTimeOffset(requestedEnd, TimeSpan.Zero),
                Status = AppointmentStatus.Confirmed,
                Type = request.Type,
                Link = new Link(GenerateTeleconsultationPath()),
                Description = request.Description ?? string.Empty,
                ScheduleSlot = slotDomain,
                Patient = patient
            };

            // 7) Persiste tudo
            await _appointmentRepository.AddAsync(appointment);

            return new ScheduleAppointmentResponse
            {
                AppointmentId = appointment.Id,
                ScheduleSlotId = slotDomain.Id,
                Link = appointment.Link.ToString()
            };
        }

        private static bool SlotFitsPolicy(
            SchedulePolicy policy,
            DateTime slotStart,
            DateTime slotEnd)
        {
            var slotDurationMinutes = (slotEnd - slotStart).TotalMinutes;

            if (Math.Abs(slotDurationMinutes - policy.DurationInMinutes) > double.Epsilon)
                return false;

            var dayOfWeek = MapSystemDayOfWeek(slotStart.DayOfWeek);
            var timeOfDayStart = TimeOnly.FromDateTime(slotStart);
            var timeOfDayEnd = TimeOnly.FromDateTime(slotEnd);

            var windowsForDay = policy.WeeklyWindows
                .Where(w => w.DayOfWeek == dayOfWeek)
                .ToList();

            if (!windowsForDay.Any())
                return false;

            bool insideAnyWindow = windowsForDay.Any(w =>
                timeOfDayStart >= w.StartTime &&
                timeOfDayEnd <= w.EndTime);

            return insideAnyWindow;
        }

        private static WeekDay MapSystemDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => WeekDay.Sunday,
                DayOfWeek.Monday => WeekDay.Monday,
                DayOfWeek.Tuesday => WeekDay.Tuesday,
                DayOfWeek.Wednesday => WeekDay.Wednesday,
                DayOfWeek.Thursday => WeekDay.Thursday,
                DayOfWeek.Friday => WeekDay.Friday,
                DayOfWeek.Saturday => WeekDay.Saturday,
                _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null)
            };
        }

        private static string GenerateTeleconsultationPath()
        {
            // Apenas o path, sem domínio, compatível com o VO Link:
            // "teleconsulta/{guid}"
            return $"teleconsulta/{Guid.NewGuid():N}";
        }
    }
}
