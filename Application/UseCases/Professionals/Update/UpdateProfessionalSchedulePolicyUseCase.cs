// Application/UseCases/Professionals/Update/UpdateProfessionalSchedulePolicyUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.Mappers;
using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Models;

namespace SGHSS.Application.UseCases.Professionals.Update
{
    /// <summary>
    /// Caso de uso responsável por atualizar a política de agendamento
    /// associada a um profissional.
    /// </summary>
    /// <remarks>
    /// Este caso de uso encapsula a lógica necessária para localizar o profissional,
    /// converter a nova política de agenda para o modelo de domínio e persistir
    /// as alterações por meio do repositório de profissionais.
    /// </remarks>
    public sealed class UpdateProfessionalSchedulePolicyUseCase
    {
        private readonly IProfessionalRepository _professionalRepository;

        /// <summary>
        /// Cria uma nova instância do caso de uso de atualização de política de agenda de profissional.
        /// </summary>
        /// <param name="professionalRepository">
        /// Repositório responsável por acessar e persistir dados de profissionais.
        /// </param>
        public UpdateProfessionalSchedulePolicyUseCase(IProfessionalRepository professionalRepository)
        {
            _professionalRepository = professionalRepository;
        }

        /// <summary>
        /// Executa o fluxo de atualização da política de agenda do profissional informado.
        /// </summary>
        /// <param name="request">
        /// Objeto contendo o identificador do profissional e a nova política de agendamento.
        /// </param>
        /// <returns>
        /// Um <see cref="UpdateProfessionalSchedulePolicyResponse"/> contendo o identificador
        /// do profissional e a política de agenda atualmente configurada.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Lançada quando o request é nulo ou quando a política de agenda não é fornecida.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Lançada quando o identificador do profissional é vazio.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando o profissional não é encontrado ou quando os dados da política
        /// de agenda são inválidos para o contexto de negócio.
        /// </exception>
        public async Task<UpdateProfessionalSchedulePolicyResponse> Handle(
            UpdateProfessionalSchedulePolicyRequest request
        )
        {
            // Validação defensiva de entrada
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request), "A requisição não pode ser nula.");
            }

            if (request.ProfessionalId == Guid.Empty)
            {
                throw new ArgumentException(
                    "O identificador do profissional não pode ser vazio.",
                    nameof(request.ProfessionalId)
                );
            }

            if (request.SchedulePolicy is null)
            {
                throw new ArgumentNullException(
                    nameof(request.SchedulePolicy),
                    "A política de agendamento não pode ser nula."
                );
            }

            if (request.SchedulePolicy.DurationInMinutes <= 0)
            {
                throw new InvalidOperationException(
                    "A duração padrão dos atendimentos deve ser maior que zero."
                );
            }

            if (string.IsNullOrWhiteSpace(request.SchedulePolicy.TimeZone))
            {
                throw new InvalidOperationException(
                    "O fuso horário da política de agenda não pode ser vazio."
                );
            }

            // Busca o profissional na base
            var professional = await _professionalRepository.GetByIdAsync(request.ProfessionalId);

            if (professional is null)
            {
                throw new InvalidOperationException(
                    "Profissional não encontrado para o identificador informado."
                );
            }

            // Converte o DTO de política para o modelo de domínio,
            // reutilizando o mesmo mapper do fluxo de registro.
            var newPolicy = SchedulePolicyMapper.ToDomain(request.SchedulePolicy);

            // Garante que exista um objeto de agenda associado ao profissional.
            if (professional.ProfessionalSchedule is null)
            {
                professional.ProfessionalSchedule = new ProfessionalSchedule
                {
                    SchedulePolicy = newPolicy
                };
            }
            else
            {
                // Atualiza apenas a política associada à agenda existente.
                professional.ProfessionalSchedule.SchedulePolicy = newPolicy;
            }

            // Persistência das alterações
            await _professionalRepository.UpdateAsync(professional);

            // Monta o response utilizando o DTO de entrada como fonte,
            // já que ele representa a política atualmente aplicada.
            return new UpdateProfessionalSchedulePolicyResponse
            {
                ProfessionalId = professional.Id,
                SchedulePolicy = new SchedulePolicyDto
                {
                    DurationInMinutes = request.SchedulePolicy.DurationInMinutes,
                    TimeZone = request.SchedulePolicy.TimeZone,
                    WeeklyWindows = request.SchedulePolicy.WeeklyWindows
                }
            };
        }
    }
}
