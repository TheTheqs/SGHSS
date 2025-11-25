// Application/UseCases/EletronicPrescriptions/Issue/IssueEletronicPrescriptionUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.UseCases.EletronicPrescriptions.Issue
{
    /// <summary>
    /// Caso de uso responsável pela emissão de uma prescrição eletrônica
    /// para um paciente.
    /// </summary>
    /// <remarks>
    /// Este caso de uso:
    /// <list type="number">
    /// <item>Valida os dados de entrada (datas, instruções e assinatura);</item>
    /// <item>Garante a existência das entidades envolvidas (paciente, profissional, unidade de saúde e consulta);</item>
    /// <item>Realiza o mapeamento dos dados primitivos para o Value Object <see cref="IcpSignature"/>;</item>
    /// <item>Cria e persiste uma instância de <see cref="EletronicPrescription"/> com os relacionamentos adequadamente configurados.</item>
    /// </list>
    /// A data de criação (<c>CreatedAt</c>) é definida internamente (UTC), garantindo consistência.
    /// </remarks>
    public sealed class IssueEletronicPrescriptionUseCase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IProfessionalRepository _professionalRepository;
        private readonly IHealthUnitRepository _healthUnitRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IEletronicPrescriptionRepository _eletronicPrescriptionRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de emissão de prescrição eletrônica.
        /// </summary>
        /// <param name="patientRepository">Repositório de pacientes.</param>
        /// <param name="professionalRepository">Repositório de profissionais de saúde.</param>
        /// <param name="healthUnitRepository">Repositório de unidades de saúde.</param>
        /// <param name="appointmentRepository">Repositório de consultas.</param>
        /// <param name="eletronicPrescriptionRepository">Repositório de prescrições eletrônicas.</param>
        public IssueEletronicPrescriptionUseCase(
            IPatientRepository patientRepository,
            IProfessionalRepository professionalRepository,
            IHealthUnitRepository healthUnitRepository,
            IAppointmentRepository appointmentRepository,
            IEletronicPrescriptionRepository eletronicPrescriptionRepository)
        {
            _patientRepository = patientRepository;
            _professionalRepository = professionalRepository;
            _healthUnitRepository = healthUnitRepository;
            _appointmentRepository = appointmentRepository;
            _eletronicPrescriptionRepository = eletronicPrescriptionRepository;
        }

        /// <summary>
        /// Executa o fluxo de emissão de uma nova prescrição eletrônica.
        /// </summary>
        /// <param name="request">Dados necessários para emissão da prescrição.</param>
        /// <returns>
        /// Um <see cref="IssueEletronicPrescriptionResponse"/> contendo os metadados
        /// da prescrição emitida.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando algum dado de entrada é inválido ou quando
        /// alguma das entidades necessárias não é encontrada.
        /// </exception>
        public async Task<IssueEletronicPrescriptionResponse> Handle(IssueEletronicPrescriptionRequest request)
        {
            // 1. Validações básicas dos dados primitivos
            if (string.IsNullOrWhiteSpace(request.Instructions))
            {
                throw new InvalidOperationException("As instruções da prescrição não podem ser vazias.");
            }

            if (string.IsNullOrWhiteSpace(request.IcpSignatureRaw))
            {
                throw new InvalidOperationException("A assinatura ICP da prescrição é obrigatória.");
            }

            var now = DateTimeOffset.UtcNow;

            if (request.ValidUntil <= now)
            {
                throw new InvalidOperationException("A data de validade da prescrição deve ser posterior à data de criação.");
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

            // Regra: garantir que a consulta pertence ao paciente informado
            if (appointment.Patient is not null && appointment.Patient.Id != patient.Id)
            {
                throw new InvalidOperationException("A consulta informada não pertence ao paciente especificado.");
            }

            // 6. Cria o Value Object de assinatura ICP
            var icpSignature = new IcpSignature(request.IcpSignatureRaw);

            // 7. Cria a entidade de domínio EletronicPrescription
            var prescription = new EletronicPrescription
            {
                CreatedAt = now,
                ValidUntil = request.ValidUntil,
                Instructions = request.Instructions,
                IcpSignature = icpSignature,
                Appointment = appointment,
                Patient = patient,
                Professional = professional,
                HealthUnit = healthUnit
            };

            // 8. Navegações inversas explícitas (opcional, mas deixa o grafo coerente em memória)
            patient.EletronicPrescriptions.Add(prescription);
            professional.EletronicPrescriptions.Add(prescription);
            healthUnit.EletronicPrescriptions.Add(prescription);
            appointment.EletronicPrescription = prescription;

            // 9. Persiste a prescrição
            await _eletronicPrescriptionRepository.AddAsync(prescription);

            // 10. Monta e retorna o response
            return new IssueEletronicPrescriptionResponse(
                prescriptionId: prescription.Id,
                patientId: patient.Id,
                professionalId: professional.Id,
                healthUnitId: healthUnit.Id,
                appointmentId: appointment.Id,
                createdAt: prescription.CreatedAt,
                validUntil: prescription.ValidUntil,
                instructions: prescription.Instructions
            );
        }
    }
}
