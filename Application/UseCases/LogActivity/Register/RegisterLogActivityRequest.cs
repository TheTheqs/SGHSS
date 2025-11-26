// Application/UseCases/LogActivities/Register/RegisterLogActivityRequest.cs

using SGHSS.Domain.Enums;
using SGHSS.Domain.ValueObjects;

namespace SGHSS.Application.UseCases.LogActivities.Register
{
    /// <summary>
    /// Representa os dados necessários para registrar uma nova atividade
    /// de log no sistema.
    /// </summary>
    public sealed class RegisterLogActivityRequest
    {
        /// <summary>
        /// Identificador do usuário responsável pela ação.
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Identificador da unidade de saúde relacionada à ação,
        /// quando aplicável.
        /// </summary>
        public Guid? HealthUnitId { get; init; }

        /// <summary>
        /// Tipo da ação registrada, como por exemplo:
        /// "RegisterPatient", "UpdateBedStatus", etc.
        /// </summary>
        public string Action { get; init; } = string.Empty;

        /// <summary>
        /// Descrição detalhada do evento ocorrido.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Endereço IP associado à origem da requisição.
        /// </summary>
        public IpAddress IpAddress { get; init; }

        /// <summary>
        /// Resultado da operação realizada.
        /// </summary>
        public LogResult Result { get; init; }
    }
}
