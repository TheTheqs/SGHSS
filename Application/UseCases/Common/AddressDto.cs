// Application/UseCases/Common/AddressDto.cs

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa os dados de endereço fornecidos pela camada de interface
    /// para criação ou atualização de entidades que utilizam o Value Object <see cref="Address"/>.
    /// </summary>
    /// <remarks>
    /// Este DTO atua como um contrato de transporte entre interface e aplicação,
    /// permitindo que o caso de uso converta os dados em um <see cref="Address"/> 
    /// devidamente validado e normalizado.
    /// Nenhuma validação profunda é executada aqui — essa responsabilidade
    /// pertence ao Value Object.
    /// </remarks>
    public sealed class AddressDto
    {
        /// <summary>
        /// Nome da rua ou logradouro.
        /// </summary>
        public string Street { get; init; } = string.Empty;

        /// <summary>
        /// Número do imóvel (pode aceitar "S/N").
        /// </summary>
        public string Number { get; init; } = string.Empty;

        /// <summary>
        /// Cidade onde o endereço está localizado.
        /// </summary>
        public string City { get; init; } = string.Empty;

        /// <summary>
        /// Unidade Federativa (UF) em formato de sigla.
        /// </summary>
        public string State { get; init; } = string.Empty;

        /// <summary>
        /// Código postal (CEP), podendo vir com ou sem máscara.
        /// </summary>
        public string Cep { get; init; } = string.Empty;

        /// <summary>
        /// Bairro ou distrito do endereço, quando aplicável.
        /// </summary>
        public string? District { get; init; }

        /// <summary>
        /// Informação complementar do endereço, como bloco ou apartamento.
        /// </summary>
        public string? Complement { get; init; }

        /// <summary>
        /// País associado ao endereço. Valor padrão esperado: "BR".
        /// </summary>
        public string Country { get; init; } = "BR";
    }
}
