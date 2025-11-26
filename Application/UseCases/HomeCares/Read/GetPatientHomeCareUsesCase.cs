// Application/UseCases/HomeCares/Read/GetPatientHomeCaresUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.HomeCares.Read
{
    /// <summary>
    /// Caso de uso responsável por consultar os registros de home care
    /// associados a um paciente específico.
    /// </summary>
    public class GetPatientHomeCaresUseCase
    {
        private readonly IHomeCareRepository _homeCareRepository;
        private readonly IPatientRepository _patientRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso para consulta de
        /// atendimentos de home care por paciente.
        /// </summary>
        /// <param name="homeCareRepository">
        /// Repositório responsável pelo acesso aos registros de home care.
        /// </param>
        /// <param name="patientRepository">
        /// Repositório responsável pelo acesso aos dados de pacientes.
        /// </param>
        public GetPatientHomeCaresUseCase(
            IHomeCareRepository homeCareRepository,
            IPatientRepository patientRepository)
        {
            _homeCareRepository = homeCareRepository;
            _patientRepository = patientRepository;
        }

        /// <summary>
        /// Recupera todos os atendimentos de home care associados ao paciente
        /// informado no request.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o identificador do paciente alvo da consulta.
        /// </param>
        /// <returns>
        /// Um <see cref="GetPatientHomeCaresResponse"/> contendo a lista de
        /// registros de home care do paciente.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request fornecido é nulo.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o paciente informado não é encontrado.
        /// </exception>
        public async Task<GetPatientHomeCaresResponse> Handle(GetPatientHomeCaresRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var patient = await _patientRepository.GetByIdAsync(request.PatientId);
            if (patient is null)
            {
                throw new InvalidOperationException("Paciente informado não foi encontrado.");
            }

            var homeCares = await _homeCareRepository.GetByPatientIdAsync(request.PatientId);

            var dtos = homeCares
                .Select(hc => new HomeCareDto
                {
                    Id = hc.Id,
                    Date = hc.Date,
                    Description = hc.Description,
                    ProfessionalId = hc.Professional.Id,
                    ProfessionalName = hc.Professional.Name,
                    HealthUnitId = hc.HealthUnit.Id,
                    HealthUnitName = hc.HealthUnit.Name
                })
                .ToArray();

            return new GetPatientHomeCaresResponse
            {
                HomeCares = dtos
            };
        }
    }
}
