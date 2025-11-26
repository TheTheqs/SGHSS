// Application/UseCases/HomeCares/Register/RegisterHomeCareUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.HomeCares.Register
{
    /// <summary>
    /// Caso de uso responsável por registrar um novo atendimento
    /// de home care para um paciente.
    /// </summary>
    /// <remarks>
    /// Este caso de uso valida a existência do paciente, do profissional
    /// e da unidade de saúde antes de criar o registro de home care,
    /// garantindo a consistência referencial dos dados.
    /// </remarks>
    public class RegisterHomeCareUseCase
    {
        private readonly IHomeCareRepository _homeCareRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IProfessionalRepository _professionalRepository;
        private readonly IHealthUnitRepository _healthUnitRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso para registro de
        /// atendimentos de home care.
        /// </summary>
        /// <param name="homeCareRepository">
        /// Repositório responsável pela persistência de registros de home care.
        /// </param>
        /// <param name="patientRepository">
        /// Repositório responsável pelo acesso aos dados de pacientes.
        /// </param>
        /// <param name="professionalRepository">
        /// Repositório responsável pelo acesso aos dados de profissionais.
        /// </param>
        /// <param name="healthUnitRepository">
        /// Repositório responsável pelo acesso às unidades de saúde.
        /// </param>
        public RegisterHomeCareUseCase(
            IHomeCareRepository homeCareRepository,
            IPatientRepository patientRepository,
            IProfessionalRepository professionalRepository,
            IHealthUnitRepository healthUnitRepository)
        {
            _homeCareRepository = homeCareRepository;
            _patientRepository = patientRepository;
            _professionalRepository = professionalRepository;
            _healthUnitRepository = healthUnitRepository;
        }

        /// <summary>
        /// Registra um novo atendimento de home care com base nas
        /// informações fornecidas no request.
        /// </summary>
        /// <param name="request">
        /// Dados necessários para criação do registro de home care.
        /// </param>
        /// <returns>
        /// Um <see cref="RegisterHomeCareResponse"/> contendo o identificador
        /// do atendimento recém-registrado.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request fornecido é nulo.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o paciente, o profissional ou a unidade de saúde
        /// informados não são encontrados.
        /// </exception>
        public async Task<RegisterHomeCareResponse> Handle(RegisterHomeCareRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            // Valida existência do paciente
            var patient = await _patientRepository.GetByIdAsync(request.PatientId);
            if (patient is null)
            {
                throw new InvalidOperationException("Paciente informado não foi encontrado.");
            }

            // Valida existência do profissional
            var professional = await _professionalRepository.GetByIdAsync(request.ProfessionalId);
            if (professional is null)
            {
                throw new InvalidOperationException("Profissional informado não foi encontrado.");
            }

            // Valida existência da unidade de saúde
            var healthUnit = await _healthUnitRepository.GetByIdAsync(request.HealthUnitId);
            if (healthUnit is null)
            {
                throw new InvalidOperationException("Unidade de saúde informada não foi encontrada.");
            }

            // Cria entidade de HomeCare
            var homeCare = new HomeCare
            {
                Date = request.Date,
                Description = request.Description ?? string.Empty,
                Patient = patient,
                Professional = professional,
                HealthUnit = healthUnit
            };

            await _homeCareRepository.AddAsync(homeCare);

            return new RegisterHomeCareResponse
            {
                HomeCareId = homeCare.Id
            };
        }
    }
}
