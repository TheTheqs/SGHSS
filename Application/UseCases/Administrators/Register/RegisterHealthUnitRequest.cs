// Application/UseCases/Administrators/Register/RegisterHealthUnitRequest.cs

using SGHSS.Application.UseCases.Common;
using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Administrators.Register
{
    /// <summary>
    /// Representa os dados necessários para registrar uma nova unidade de saúde no sistema.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado na camada de Application para transportar informações
    /// fornecidas pelo usuário ou pela interface externa, permitindo que o caso de uso
    /// de registro valide e processe a criação da unidade.
    /// </remarks>
    public sealed class RegisterHealthUnitRequest
    {
        /// <summary>
        /// Nome oficial da unidade de saúde.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// CNPJ da unidade de saúde.
        /// </summary>
        public string Cnpj { get; init; } = string.Empty;

        /// <summary>
        /// Número de telefone para contato institucional.
        /// </summary>
        public string Phone { get; init; } = string.Empty;

        /// <summary>
        /// Endereço completo da unidade, representado por um DTO de endereço.
        /// </summary>
        public AddressDto Address { get; init; } = null!;

        /// <summary>
        /// Tipo da unidade de saúde (ex.: hospital, UBS, laboratório, etc.).
        /// </summary>
        public HealthUnitType Type { get; init; }
    }
}
