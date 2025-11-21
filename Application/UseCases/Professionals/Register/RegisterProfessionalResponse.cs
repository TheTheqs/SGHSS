// Application/UseCases/Professionals/Register/RegisterProfessionalResponse.cs

namespace SGHSS.Application.UseCases.Professionals.Register
{
    /// <summary>
    /// Representa os dados de saída retornados após o registro bem-sucedido de um profissional.
    /// </summary>
    /// <remarks>
    /// Este DTO pode ser expandido para incluir informações adicionais, como dados de auditoria
    /// ou um resumo do profissional. No momento, expõe apenas o identificador gerado.
    /// </remarks>
    public sealed class RegisterProfessionalResponse
    {
        /// <summary>
        /// Identificador único do profissional recém-registrado.
        /// </summary>
        public Guid ProfessionalId { get; }

        /// <summary>
        /// Cria uma nova instância de resposta para o caso de uso de registro de profissional.
        /// </summary>
        /// <param name="professionalId">Identificador do profissional criado.</param>
        public RegisterProfessionalResponse(Guid professionalId)
        {
            ProfessionalId = professionalId;
        }
    }
}
