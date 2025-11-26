// Application/UseCases/ProfessionalSchedules/Consult/ConsultReservedScheduleSlotsRequest.cs

namespace SGHSS.Application.UseCases.ProfessionalSchedules.Consult
{
    /// <summary>
    /// Representa a requisição utilizada para consultar todos os horários
    /// reservados associados a uma agenda profissional específica.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado como parâmetro de entrada no caso de uso
    /// <see cref="ConsultReservedScheduleSlotsUseCase"/>, permitindo que a
    /// camada de aplicação recupere os horários ocupados de uma determinada
    /// agenda sem expor a lógica interna de persistência.
    /// </remarks>
    public sealed class ConsultReservedScheduleSlotsRequest
    {
        /// <summary>
        /// Identificador único da agenda profissional cujos horários
        /// reservados serão consultados.
        /// </summary>
        /// <remarks>
        /// Este valor deve corresponder a um registro válido na tabela
        /// <c>ProfessionalSchedules</c>. Caso não exista, o caso de uso
        /// correspondente deve retornar a falha apropriada.
        /// </remarks>
        public Guid ProfessionalScheduleId { get; init; }
    }
}
