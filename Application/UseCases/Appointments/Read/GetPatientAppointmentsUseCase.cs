// Application/UseCases/Appointments/Read/GetPatientAppointmentsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Appointments.Read
{
    /// <summary>
    /// Caso de uso responsável por recuperar todas as consultas (appointments)
    /// associadas a um paciente específico.
    /// </summary>
    /// <remarks>
    /// Este caso de uso:
    /// <list type="number">
    /// <item>Valida a existência do paciente informado;</item>
    /// <item>Consulta todas as consultas vinculadas ao paciente;</item>
    /// <item>Projeta as entidades de domínio para <see cref="AppointmentDto"/>;</item>
    /// <item>Retorna um <see cref="GetPatientAppointmentsResponse"/> contendo os dados consolidados.</item>
    /// </list>
    /// </remarks>
    public sealed class GetPatientAppointmentsUseCase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de consulta de consultas de um paciente.
        /// </summary>
        /// <param name="patientRepository">Repositório de pacientes.</param>
        /// <param name="appointmentRepository">Repositório de consultas.</param>
        public GetPatientAppointmentsUseCase(
            IPatientRepository patientRepository,
            IAppointmentRepository appointmentRepository)
        {
            _patientRepository = patientRepository;
            _appointmentRepository = appointmentRepository;
        }

        /// <summary>
        /// Executa a consulta de todas as consultas associadas a um paciente.
        /// </summary>
        /// <param name="request">Dados necessários para identificar o paciente.</param>
        /// <returns>
        /// Um <see cref="GetPatientAppointmentsResponse"/> contendo o identificador
        /// do paciente e a lista de suas consultas.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando a requisição informada é nula.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o paciente informado não é encontrado.
        /// </exception>
        public async Task<GetPatientAppointmentsResponse> Handle(GetPatientAppointmentsRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // 1. Valida existência do paciente
            var patient = await _patientRepository.GetByIdAsync(request.PatientId);

            if (patient is null)
            {
                throw new InvalidOperationException("Paciente não encontrado para o identificador informado.");
            }

            // 2. Recupera todas as consultas do paciente
            var appointments = await _appointmentRepository.GetAllByPatientIdAsync(request.PatientId);

            // 3. Projeta as entidades para DTOs
            var appointmentDtos = appointments
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status,
                    Type = a.Type,
                    Link = a.Link?.ToString(),
                    Description = a.Description,
                    HasMedicalRecordUpdate = a.MedicalRecordUpdate is not null,
                    HasEletronicPrescription = a.EletronicPrescription is not null,
                    HasDigitalMedicalCertificate = a.DigitalMedicalCertificate is not null
                })
                .ToList()
                .AsReadOnly();

            // 4. Monta o response
            return new GetPatientAppointmentsResponse(
                patientId: request.PatientId,
                appointments: appointmentDtos);
        }
    }
}
