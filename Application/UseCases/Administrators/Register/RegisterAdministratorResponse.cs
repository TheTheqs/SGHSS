// Application/UseCases/Administrators/Register/RegisterAdministratorResponse.cs

namespace SGHSS.Application.UseCases.Administrators.Register
{
    /// <summary>
    /// Representa a resposta retornada após o registro de um novo Administrador,
    /// contendo o identificador único gerado para a entidade criada.
    /// </summary>
    public sealed class RegisterAdministratorResponse
    {
        /// <summary>
        /// Identificador único do Administrador recém-registrado.
        /// </summary>
        public Guid AdministratorId { get; }

        /// <summary>
        /// Inicializa uma nova instância da resposta de registro de Administrador.
        /// </summary>
        /// <param name="administratorId">O ID gerado para o Administrador criado.</param>
        public RegisterAdministratorResponse(Guid administratorId)
        {
            AdministratorId = administratorId;
        }
    }
}
