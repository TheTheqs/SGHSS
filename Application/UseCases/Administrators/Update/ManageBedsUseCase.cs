// Application/UseCases/Administrators/Update/ManageBedsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.Mappers;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Administrators.Update
{
    /// <summary>
    /// Caso de uso responsável por gerenciar a capacidade de leitos
    /// de uma unidade de saúde existente (adição e remoção).
    /// </summary>
    /// <remarks>
    /// Este caso de uso atua sobre o agregado HealthUnit, permitindo
    /// incrementar ou reduzir a lista de leitos conforme o modelo
    /// informado em <see cref="ManageBedsRequest"/>. Leitos ocupados
    /// não podem ser manipulados diretamente por este fluxo.
    /// </remarks>
    public class ManageBedsUseCase
    {
        private readonly IHealthUnitRepository _healthUnitRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de gerenciamento de leitos.
        /// </summary>
        /// <param name="healthUnitRepository">
        /// Repositório responsável pelo acesso e persistência da unidade de saúde.
        /// </param>
        public ManageBedsUseCase(IHealthUnitRepository healthUnitRepository)
        {
            _healthUnitRepository = healthUnitRepository;
        }

        /// <summary>
        /// Executa a operação de gerenciamento de leitos (adição ou remoção)
        /// de acordo com os parâmetros definidos na requisição.
        /// </summary>
        /// <param name="request">Dados da operação de gerenciamento de leitos.</param>
        /// <returns>
        /// Um <see cref="ManageBedsResponse"/> contendo o identificador da unidade
        /// afetada e a lista atualizada de leitos.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a quantidade é inválida, o status do leito é Occupied
        /// ou quando não há leitos suficientes para remoção.
        /// </exception>
        public async Task<ManageBedsResponse> Handle(ManageBedsRequest request)
        {
            // Regra de negócio: quantidade deve ser positiva
            if (request.Quantity < 1)
            {
                throw new InvalidOperationException(
                    "A quantidade de leitos deve ser maior ou igual a 1."
                );
            }

            // Regra de negócio: leitos ocupados não podem ser manipulados diretamente
            if (request.Bed.Status == BedStatus.Occupied)
            {
                throw new InvalidOperationException(
                    "Leitos com status 'Occupied' não podem ser adicionados ou removidos manualmente."
                );
            }

            // Carrega a unidade de saúde alvo
            var healthUnit = await _healthUnitRepository.GetByIdAsync(request.HealthUnitId);
            if (healthUnit is null)
            {
                throw new InvalidOperationException(
                    "Unidade de saúde não encontrada para o identificador informado."
                );
            }

            // Direciona a operação conforme o tipo (adição ou remoção)
            return request.IsAdding
                ? await AddBedsAsync(healthUnit, request)
                : await RemoveBedsAsync(healthUnit, request);
        }

        /// <summary>
        /// Adiciona novos leitos à unidade de saúde com base no modelo informado.
        /// </summary>
        /// <param name="healthUnit">Unidade de saúde que terá seus leitos incrementados.</param>
        /// <param name="request">Dados da operação, incluindo modelo e quantidade.</param>
        /// <returns>Resposta contendo a lista atualizada de leitos.</returns>
        private async Task<ManageBedsResponse> AddBedsAsync(HealthUnit healthUnit, ManageBedsRequest request)
        {
            // Gera as entidades de leito com base no DTO fornecido
            for (int i = 0; i < request.Quantity; i++)
            {
                Bed newBed = request.Bed.ToDomain();
                healthUnit.Beds.Add(newBed);
            }

            // Persiste as alterações na unidade de saúde
            await _healthUnitRepository.UpdateAsync(healthUnit);
            var bedDtos = healthUnit.Beds
            .Select(b => new BedDto
            {
                BedId = b.Id,
                BedNumber = b.BedNumber,
                Type = b.Type,
                Status = b.Status
            })
            .ToList();

            // Monta resposta com o estado atual dos leitos
            return new ManageBedsResponse(healthUnit.Id, bedDtos);
        }

        /// <summary>
        /// Remove leitos da unidade de saúde com base no modelo e quantidade informados.
        /// </summary>
        /// <param name="healthUnit">Unidade de saúde que terá seus leitos reduzidos.</param>
        /// <param name="request">Dados da operação, incluindo modelo e quantidade.</param>
        /// <returns>Resposta contendo a lista atualizada de leitos.</returns>
        private async Task<ManageBedsResponse> RemoveBedsAsync(HealthUnit healthUnit, ManageBedsRequest request)
        {
            // Seleciona os leitos que correspondem ao modelo informado
            var matchingBeds = healthUnit.Beds
                .Where(b => b.Type == request.Bed.Type && b.Status == request.Bed.Status)
                .ToList();

            // Regra de negócio: não permitir remoção maior que o número disponível
            if (matchingBeds.Count < request.Quantity)
            {
                throw new InvalidOperationException(
                    "A unidade de saúde não possui leitos suficientes para serem removidos com os critérios informados."
                );
            }

            // Remove a quantidade solicitada de leitos compatíveis
            for (int i = 0; i < request.Quantity; i++)
            {
                healthUnit.Beds.Remove(matchingBeds[i]);
            }

            // Persiste as alterações
            await _healthUnitRepository.UpdateAsync(healthUnit);
            var bedDtos = healthUnit.Beds
            .Select(b => new BedDto
            {
                BedId = b.Id,
                BedNumber = b.BedNumber,
                Type = b.Type,
                Status = b.Status
            })
            .ToList();

            // Monta resposta com o estado atual dos leitos
            return new ManageBedsResponse(healthUnit.Id, bedDtos);
        }

        public async Task<ManageBedsResponse> RemoveBedByNumberAsync(Guid healthUnitId, string bedNumber)
        {
            if (string.IsNullOrWhiteSpace(bedNumber))
            {
                throw new ArgumentException(
                    "O número do leito não pode ser vazio.",
                    nameof(bedNumber)
                );
            }

            var healthUnit = await _healthUnitRepository.GetByIdAsync(healthUnitId);
            if (healthUnit is null)
            {
                throw new InvalidOperationException(
                    "Unidade de saúde não encontrada para o identificador informado."
                );
            }

            // Encontra o primeiro leito com o número informado
            var bed = healthUnit.Beds
                .FirstOrDefault(b => b.BedNumber == bedNumber);

            if (bed is null)
            {
                throw new InvalidOperationException(
                    "A unidade de saúde não possui um leito com o número informado."
                );
            }

            // Reaproveita a mesma regra de consistência: não remover leito ocupado
            if (bed.Status == BedStatus.Occupied)
            {
                throw new InvalidOperationException(
                    "Não é possível remover um leito que está com status 'Occupied'."
                );
            }

            healthUnit.Beds.Remove(bed);

            await _healthUnitRepository.UpdateAsync(healthUnit);
            var bedDtos = healthUnit.Beds
            .Select(b => new BedDto
            {
                BedId = b.Id,
                BedNumber = b.BedNumber,
                Type = b.Type,
                Status = b.Status
            })
            .ToList();

            return new ManageBedsResponse(healthUnit.Id, bedDtos);
        }
    }
}
