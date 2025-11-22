// Application/UseCases/Administrators/Register/RegisterHealthUnitResponse.cs

namespace SGHSS.Application.UseCases.Administrators.Register
{
    /// <summary>
    /// Representa a resposta retornada após o registro bem-sucedido de uma unidade de saúde.
    /// </summary>
    /// <remarks>
    /// Contém apenas o identificador único da unidade criada, permitindo
    /// que camadas superiores façam redirecionamentos, exibição ou operações adicionais.
    /// </remarks>
    public class RegisterHealthUnitResponse
    {
        /// <summary>
        /// Identificador da unidade de saúde recém-registrada.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Cria uma instância da resposta de registro contendo o ID gerado.
        /// </summary>
        /// <param name="healthUnitId">O ID da unidade criada.</param>
        public RegisterHealthUnitResponse(Guid healthUnitId)
        {
            HealthUnitId = healthUnitId;
        }
    }
}
