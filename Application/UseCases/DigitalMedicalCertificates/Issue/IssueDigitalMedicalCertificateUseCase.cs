// Application/UseCases/DigitalMedicalCertificates/Issue/IssueDigitalMedicalCertificateUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.UseCases.DigitalMedicalCertificates.Issue
{
    /// <summary>
    /// Caso de uso responsável pela emissão de um atestado médico digital
    /// para um paciente.
    /// </summary>
    /// <remarks>
    /// Este caso de uso:
    /// <list type="number">
    /// <item>Valida os dados de entrada (datas, recomendações e assinatura);</item>
    /// <item>Garante a existência das entidades envolvidas (paciente, profissional, unidade de saúde e consulta);</item>
    /// <item>Realiza o mapeamento dos dados primitivos para o Value Object <see cref="IcpSignature"/>;</item>
    /// <item>Cria e persiste uma instância de <see cref="DigitalMedicalCertificate"/> com os relacionamentos adequadamente configurados.</item>
    /// </list>
    /// A data de emissão (<c>IssuedAt</c>) é definida internamente (UTC), garantindo consistência.
    /// </remarks>
    public sealed class IssueDigitalMedicalCertificateUseCase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IProfessionalRepository _professionalRepository;
        private readonly IHealthUnitRepository _healthUnitRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDigitalMedicalCertificateRepository _digitalMedicalCertificateRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de emissão de atestado médico digital.
        /// </summary>
        /// <param name="patientRepository">Repositório de pacientes.</param>
        /// <param name="professionalRepository">Repositório de profissionais de saúde.</param>
        /// <param name="healthUnitRepository">Repositório de unidades de saúde.</param>
        /// <param name="appointmentRepository">Repositório de consultas.</param>
        /// <param name="digitalMedicalCertificateRepository">Repositório de atestados médicos digitais.</param>
        public IssueDigitalMedicalCertificateUseCase(
            IPatientRepository patientRepository,
            IProfessionalRepository professionalRepository,
            IHealthUnitRepository healthUnitRepository,
            IAppointmentRepository appointmentRepository,
            IDigitalMedicalCertificateRepository digitalMedicalCertificateRepository)
        {
            _patientRepository = patientRepository;
            _professionalRepository = professionalRepository;
            _healthUnitRepository = healthUnitRepository;
            _appointmentRepository = appointmentRepository;
            _digitalMedicalCertificateRepository = digitalMedicalCertificateRepository;
        }

        /// <summary>
        /// Executa o fluxo de emissão de um novo atestado médico digital.
        /// </summary>
        /// <param name="request">Dados necessários para emissão do atestado.</param>
        /// <returns>
        /// Um <see cref="IssueDigitalMedicalCertificateResponse"/> contendo os metadados
        /// do atestado emitido.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando algum dado de entrada é inválido ou quando
        /// alguma das entidades necessárias não é encontrada.
        /// </exception>
        public async Task<IssueDigitalMedicalCertificateResponse> Handle(IssueDigitalMedicalCertificateRequest request)
        {
            // 1. Validações básicas dos dados primitivos
            if (string.IsNullOrWhiteSpace(request.Recommendations))
            {
                throw new InvalidOperationException("As recomendações do atestado não podem ser vazias.");
            }

            if (string.IsNullOrWhiteSpace(request.IcpSignatureRaw))
            {
                throw new InvalidOperationException("A assinatura ICP do atestado é obrigatória.");
            }

            var now = DateTimeOffset.UtcNow;

            if (request.ValidUntil <= now)
            {
                throw new InvalidOperationException("A data de validade do atestado deve ser posterior à data de emissão.");
            }

            // 2. Carrega o paciente
            var patient = await _patientRepository.GetByIdAsync(request.PatientId);

            if (patient is null)
            {
                throw new InvalidOperationException("Paciente não encontrado para o identificador informado.");
            }

            // 3. Carrega o profissional
            var professional = await _professionalRepository.GetByIdAsync(request.ProfessionalId);

            if (professional is null)
            {
                throw new InvalidOperationException("Profissional não encontrado para o identificador informado.");
            }

            // 4. Carrega a unidade de saúde
            var healthUnit = await _healthUnitRepository.GetByIdAsync(request.HealthUnitId);

            if (healthUnit is null)
            {
                throw new InvalidOperationException("Unidade de saúde não encontrada para o identificador informado.");
            }

            // 5. Carrega a consulta associada
            var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId);

            if (appointment is null)
            {
                throw new InvalidOperationException("Consulta não encontrada para o identificador informado.");
            }

            // (Regra opcional, se você quiser ser mais rígido no futuro:)
            // - Garantir que o Appointment pertença ao Patient informado.
            if (appointment.Patient is not null && appointment.Patient.Id != patient.Id)
            {
                throw new InvalidOperationException("A consulta informada não pertence ao paciente especificado.");
            }

            // 6. Cria o Value Object de assinatura ICP
            var icpSignature = new IcpSignature(request.IcpSignatureRaw);

            // 7. Cria a entidade de domínio DigitalMedicalCertificate
            var certificate = new DigitalMedicalCertificate
            {
                IssuedAt = now,
                ValidUntil = request.ValidUntil,
                Recommendations = request.Recommendations,
                IcpSignature = icpSignature,
                Appointment = appointment,
                Patient = patient,
                Professional = professional,
                HealthUnit = healthUnit
            };

            // 8. (Opcional) reforça navegação inversa, caso deseje garantir em memória.
            // EF geralmente faz o *fix-up* sozinho com o rastreamento,
            // mas não custa nada deixar explícito se você gosta de clareza.
            patient.DigitalMedicalCertificates.Add(certificate);
            professional.DigitalMedicalCertificates.Add(certificate);
            healthUnit.DigitalMedicalCertificates.Add(certificate);
            appointment.DigitalMedicalCertificate = certificate;

            // 9. Persiste o atestado
            await _digitalMedicalCertificateRepository.AddAsync(certificate);

            // 10. Monta e retorna o response
            return new IssueDigitalMedicalCertificateResponse(
                certificateId: certificate.Id,
                patientId: patient.Id,
                professionalId: professional.Id,
                healthUnitId: healthUnit.Id,
                appointmentId: appointment.Id,
                issuedAt: certificate.IssuedAt,
                validUntil: certificate.ValidUntil,
                recommendations: certificate.Recommendations
            );
        }
    }
}
