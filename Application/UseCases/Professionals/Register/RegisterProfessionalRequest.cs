// Application/UseCases/Professionals/Register/RegisterProfessionalRequest.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Professionals.Register
{
    /// <summary>
    /// Representa os dados de entrada necessários para registrar um novo profissional no sistema.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado pela camada de Application para receber as informações enviadas
    /// pela camada de interface (por exemplo, uma API) e orquestrar o caso de uso de registro
    /// de profissionais, sem expor diretamente as entidades de domínio.
    /// </remarks>
    public sealed class RegisterProfessionalRequest
    {
        /// <summary>
        /// Nome completo do profissional.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Endereço de e-mail do profissional.
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Senha pura fornecida no cadastro, que será transformada em um Value Object Password no UseCase.
        /// </summary>
        public string Password { get; init; } = string.Empty;

        /// <summary>
        /// Número de telefone do profissional (em formato normalizado pela camada de interface).
        /// </summary>
        public string Phone { get; init; } = string.Empty;

        /// <summary>
        /// License de registro em conselho do profissional em formato textual.
        /// </summary>
        public string License { get; init; } = string.Empty;

        /// <summary>
        /// Especialidade do profissional.
        /// </summary>
        public string Specialty { get; init; } = string.Empty;

        /// <summary>
        /// Lista de consentimentos associados ao contexto atual.
        /// </summary>
        public ICollection<ConsentDto> Consents { get; init; } = new List<ConsentDto>();

        /// <summary>
        /// Política de agendamento associada ao profissional.
        /// </summary>
        public SchedulePolicyDto SchedulePolicy { get; init; } = null!;

    }
}
