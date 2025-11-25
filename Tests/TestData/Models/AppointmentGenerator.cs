// Tests/TestData/Models/AppointmentGenerator.cs

using SGHSS.Application.UseCases.Appointments.Register;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;

namespace SGHSS.Tests.TestData.Models
{
    namespace SGHSS.Tests.TestData.Appointments
    {
        /// <summary>
        /// Classe utilitária para geração de dados de entrada de agendamento
        /// (ScheduleAppointmentRequest) com valores válidos para uso em testes.
        /// </summary>
        public static class AppointmentGenerator
        {
            private static readonly Random Rng = new();

            public static ScheduleAppointmentRequest GenerateAppointment(
                Guid? professionalId = null,
                Guid? patientId = null,
                ScheduleSlotDto? slot = null,
                AppointmentType? type = null,
                string? description = null)
            {
                return new ScheduleAppointmentRequest
                {
                    ProfessionalId = professionalId ?? Guid.NewGuid(),
                    PatientId = patientId ?? Guid.NewGuid(),
                    Slot = slot ?? GenerateDefaultSlotDto(),
                    Type = type ?? PickRandomAppointmentType(),
                    Description = description ?? "Consulta agendada para avaliação."
                };
            }

            private static ScheduleSlotDto GenerateDefaultSlotDto()
            {
                DateTime start = DateTime.UtcNow.Date.AddHours(9);
                DateTime end = start.AddMinutes(30);

                return new ScheduleSlotDto
                {
                    StartDateTime = start,
                    EndDateTime = end,
                    Status = ScheduleSlotStatus.Available
                };
            }

            private static AppointmentType PickRandomAppointmentType()
            {
                var values = Enum.GetValues<AppointmentType>();
                return values[Rng.Next(values.Length)];
            }
        }
    }
}

