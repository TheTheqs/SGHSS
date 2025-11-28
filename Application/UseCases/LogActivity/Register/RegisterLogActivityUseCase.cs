// Application/UseCases/LogActivities/Register/RegisterLogActivityUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.LogActivities.Register
{
    /// <summary>
    /// Caso de uso responsável por registrar uma nova atividade de log no sistema.
    /// </summary>
    /// <remarks>
    /// Este caso de uso encapsula a lógica de validação dos dados de entrada,
    /// resolução das entidades de usuário e unidade de saúde relacionadas
    /// e a persistência do registro de log por meio do repositório de logs.
    /// </remarks>
    public class RegisterLogActivityUseCase
    {
        private readonly ILogActivityRepository _logActivityRepository;
        private readonly IUserRepository _userRepository;
        private readonly IHealthUnitRepository _healthUnitRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de registro de atividades de log.
        /// </summary>
        /// <param name="logActivityRepository">
        /// Repositório responsável por persistir as atividades de log.
        /// </param>
        /// <param name="userRepository">
        /// Repositório responsável pelo acesso às entidades de usuário.
        /// </param>
        /// <param name="healthUnitRepository">
        /// Repositório responsável pelo acesso às unidades de saúde.
        /// </param>
        public RegisterLogActivityUseCase(
            ILogActivityRepository logActivityRepository,
            IUserRepository userRepository,
            IHealthUnitRepository healthUnitRepository)
        {
            _logActivityRepository = logActivityRepository;
            _userRepository = userRepository;
            _healthUnitRepository = healthUnitRepository;
        }

        /// <summary>
        /// Executa o fluxo de registro de uma nova atividade de log.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo as informações da atividade a ser registrada.
        /// </param>
        /// <returns>
        /// Um <see cref="RegisterLogActivityResponse"/> contendo o identificador
        /// do log recém-registrado.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request é nulo.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Lançada quando o identificador de usuário é vazio ou a ação é inválida.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o usuário informado não é encontrado.
        /// </exception>
        public async Task<RegisterLogActivityResponse> Handle(RegisterLogActivityRequest request)
        {
            // Validação defensiva básica
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request), "A requisição não pode ser nula.");
            }

            if (request.UserId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O identificador do usuário não pode ser vazio.",
                    nameof(request.UserId));
            }

            if (string.IsNullOrWhiteSpace(request.Action))
            {
                throw new ArgumentException("A ação registrada no log não pode ser vazia.");
            }

            // Resolve o usuário responsável pela ação
            User? user = null;

            if (request.UserId.HasValue)
            {
                user = await _userRepository.GetByIdAsync(request.UserId.Value);
            }

            // Resolve, se informado, a unidade de saúde relacionada
            HealthUnit? healthUnit = null;

            if (request.HealthUnitId.HasValue)
            {
                healthUnit = await _healthUnitRepository.GetByIdAsync(request.HealthUnitId.Value);
                // Se não encontrar, a operação de log continua, apenas sem a associação da unidade.
            }

            // Monta a entidade de log
            var logActivity = new LogActivity
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTimeOffset.UtcNow,
                Action = request.Action,
                Description = request.Description,
                IpAddress = request.IpAddress,
                Result = request.Result,
                User = user,
                HealthUnit = healthUnit
            };

            // Persiste o registro
            await _logActivityRepository.AddAsync(logActivity);

            // Retorna o identificador do log criado
            return new RegisterLogActivityResponse
            {
                LogActivityId = logActivity.Id
            };
        }
    }
}
