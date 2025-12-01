// Application/UseCases/DigitalMedicalCertificates/GetPatientDigitalMedicalCertificates.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.DigitalMedicalCertificates.Read
{
    /// <summary>
    /// Caso de uso responsável por consultar todos os atestados médicos digitais
    /// associados a um paciente específico.
    /// </summary>
    /// <remarks>
    /// Este caso de uso realiza:
    /// <list type="bullet">
    /// <item>Validação básica do request;</item>
    /// <item>Consulta ao repositório de atestados utilizando o identificador do paciente;</item>
    /// <item>Mapeamento das entidades de domínio para <see cref="DigitalMedicalCertificateDto"/>;</item>
    /// <item>Montagem de um <see cref="GetPatientMedicalCertificatesResponse"/> com a lista resultante.</item>
    /// </list>
    /// Regras de autorização devem ser tratadas na camada de controle
    /// (por exemplo, garantindo que um paciente só veja seus próprios atestados).
    /// </remarks>
    public sealed class GetPatientMedicalCertificatesUseCase
    {
        private readonly IDigitalMedicalCertificateRepository _digitalMedicalCertificateRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso para consulta de
        /// atestados médicos digitais de um paciente.
        /// </summary>
        /// <param name="digitalMedicalCertificateRepository">
        /// Repositório responsável pelo acesso aos atestados médicos digitais.
        /// </param>
        public GetPatientMedicalCertificatesUseCase(
            IDigitalMedicalCertificateRepository digitalMedicalCertificateRepository)
        {
            _digitalMedicalCertificateRepository = digitalMedicalCertificateRepository;
        }

        /// <summary>
        /// Executa a consulta de atestados médicos digitais associados ao paciente informado.
        /// </summary>
        /// <param name="request">
        /// Dados necessários para identificar o paciente cujos atestados serão retornados.
        /// </param>
        /// <returns>
        /// Um <see cref="GetPatientMedicalCertificatesResponse"/> contendo o identificador
        /// do paciente e a lista de atestados médicos digitais emitidos para ele.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o <paramref name="request"/> é nulo.
        /// </exception>
        public async Task<GetPatientMedicalCertificatesResponse> Handle(
            GetPatientMedicalCertificatesRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var certificates = await _digitalMedicalCertificateRepository
                .GetByPatientIdAsync(request.PatientId);

            // Mapeamento domínio → DTO
            var dtoList = certificates
                .Select(c => new DigitalMedicalCertificateDto
                {
                    CertificateId = c.Id,
                    PatientId = c.Patient?.Id ?? request.PatientId,
                    ProfessionalId = c.Professional?.Id ?? Guid.Empty,
                    HealthUnitId = c.HealthUnit?.Id ?? Guid.Empty,
                    AppointmentId = c.Appointment?.Id ?? Guid.Empty,
                    CreatedAt = c.IssuedAt,
                    ValidUntil = c.ValidUntil,
                    Recommendations = c.Recommendations
                })
                .ToList()
                .AsReadOnly();

            return new GetPatientMedicalCertificatesResponse
            {
                PatientId = request.PatientId,
                Certificates = dtoList
            };
        }
    }
}
