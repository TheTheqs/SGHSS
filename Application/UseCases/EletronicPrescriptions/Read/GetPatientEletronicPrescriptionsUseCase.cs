// Application/UseCases/EletronicPrescriptions/GetPatientEletronicPrescriptionsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.EletronicPrescriptions.Read
{
    /// <summary>
    /// Caso de uso responsável por consultar todas as prescrições eletrônicas
    /// associadas a um paciente específico.
    /// </summary>
    /// <remarks>
    /// Este caso de uso realiza:
    /// <list type="bullet">
    /// <item>Validação básica do request;</item>
    /// <item>Consulta ao repositório de prescrições eletrônicas pelo identificador do paciente;</item>
    /// <item>Mapeamento das entidades de domínio para <see cref="EletronicPrescriptionDto"/>;</item>
    /// <item>Montagem de um <see cref="GetPatientEletronicPrescriptionsResponse"/> com a lista resultante.</item>
    /// </list>
    /// Regras de autorização devem ser tratadas na camada de controle
    /// (por exemplo, garantindo que um paciente só veja suas próprias prescrições).
    /// </remarks>
    public sealed class GetPatientEletronicPrescriptionsUseCase
    {
        private readonly IEletronicPrescriptionRepository _eletronicPrescriptionRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso para consulta de
        /// prescrições eletrônicas de um paciente.
        /// </summary>
        /// <param name="eletronicPrescriptionRepository">
        /// Repositório responsável pelo acesso às prescrições eletrônicas.
        /// </param>
        public GetPatientEletronicPrescriptionsUseCase(
            IEletronicPrescriptionRepository eletronicPrescriptionRepository)
        {
            _eletronicPrescriptionRepository = eletronicPrescriptionRepository;
        }

        /// <summary>
        /// Executa a consulta de prescrições eletrônicas associadas ao paciente informado.
        /// </summary>
        /// <param name="request">
        /// Dados necessários para identificar o paciente cujas prescrições serão retornadas.
        /// </param>
        /// <returns>
        /// Um <see cref="GetPatientEletronicPrescriptionsResponse"/> contendo o identificador
        /// do paciente e a lista de prescrições eletrônicas emitidas para ele.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o <paramref name="request"/> é nulo.
        /// </exception>
        public async Task<GetPatientEletronicPrescriptionsResponse> Handle(
            GetPatientEletronicPrescriptionsRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var prescriptions = await _eletronicPrescriptionRepository
                .GetByPatientIdAsync(request.PatientId);

            var dtoList = prescriptions
                .Select(p => new EletronicPrescriptionDto
                {
                    PrescriptionId = p.Id,
                    PatientId = p.Patient?.Id ?? request.PatientId,
                    ProfessionalId = p.Professional?.Id ?? Guid.Empty,
                    HealthUnitId = p.HealthUnit?.Id ?? Guid.Empty,
                    AppointmentId = p.Appointment?.Id ?? Guid.Empty,
                    CreatedAt = p.CreatedAt,
                    ValidUntil = p.ValidUntil,
                    Instructions = p.Instructions
                })
                .ToList()
                .AsReadOnly();

            return new GetPatientEletronicPrescriptionsResponse
            {
                PatientId = request.PatientId,
                Prescriptions = dtoList
            };
        }
    }
}
