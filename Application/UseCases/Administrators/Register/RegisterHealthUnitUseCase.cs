// Application/UseCases/Administrators/Register/RegisterHealthUnitUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.Mappers;
using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.UseCases.Administrators.Register
{
    /// <summary>
    /// Caso de uso responsável por registrar uma nova unidade de saúde no sistema.
    /// </summary>
    /// <remarks>
    /// Este caso de uso executa validações básicas (como verificação de duplicidade de CNPJ),
    /// instancia os Value Objects necessários e delega a persistência ao repositório correspondente.
    /// </remarks>
    public class RegisterHealthUnitUseCase
    {
        private readonly IHealthUnitRepository _healthUnitRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso para registro de unidades de saúde.
        /// </summary>
        /// <param name="healthUnitRepository">Repositório responsável pela persistência da entidade.</param>
        public RegisterHealthUnitUseCase(IHealthUnitRepository healthUnitRepository)
        {
            _healthUnitRepository = healthUnitRepository;
        }

        /// <summary>
        /// Manipula o fluxo de criação de uma nova unidade de saúde,
        /// realizando validações e persistindo a entidade.
        /// </summary>
        /// <param name="request">Dados fornecidos para criação da unidade.</param>
        /// <returns>Um objeto contendo o identificador da unidade criada.</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o CNPJ informado já estiver cadastrado.
        /// </exception>
        public async Task<RegisterHealthUnitResponse> Handle(RegisterHealthUnitRequest request)
        {
            // Instanciação dos Value Objects
            var cnpj = new Cnpj(request.Cnpj);
            var phone = new Phone(request.Phone);
            var address = request.Address.ToDomain();

            // Regra de negócio: não permitir CNPJ duplicado
            bool cnpjAlreadyExists = await _healthUnitRepository.ExistsByCnpjAsync(cnpj);
            if (cnpjAlreadyExists)
            {
                throw new InvalidOperationException(
                    "Já existe uma unidade de saúde cadastrada com o CNPJ informado."
                );
            }

            // Instanciação da entidade principal
            var healthUnit = new Domain.Models.HealthUnit
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Cnpj = cnpj,
                Phone = phone,
                Type = request.Type,
                Address = address
            };

            // Persistência
            await _healthUnitRepository.AddAsync(healthUnit);

            return new RegisterHealthUnitResponse(healthUnit.Id);
        }
    }
}

/*
 * NOTA IMPORTANTE:
 * ---------------
 * A criação de camas (Beds) e itens de estoque (InventoryItem) NÃO ocorre
 * durante o registro inicial da unidade de saúde.
 *
 * Esses recursos devem ser adicionados ou atualizados posteriormente via 
 * casos de uso específicos (Update/AddBeds, Update/AddInventoryItems), 
 * após a unidade já existir no sistema.
 *
 * Isso evita acoplamento indevido no fluxo de criação e mantém a responsabilidade
 * de cada caso de uso bem delimitada.
 */
