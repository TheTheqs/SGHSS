// Application/UseCases/Common/BedDto.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa os dados de um leito utilizados na camada de Application,
    /// permitindo transportar informações entre casos de uso, repositórios
    /// e camada de interface.
    /// </summary>
    /// <remarks>
    /// O BedDto abstrai detalhes da entidade de domínio e é utilizado em
    /// operações como adição, remoção e consulta de leitos. 
    /// Os atributos aqui presentes identificam o leito, seu tipo e seu estado
    /// atual (disponível, ocupado, manutenção etc.), conforme regras definidas
    /// pelo domínio.
    /// </remarks>
    public sealed class BedDto
    {
        /// <summary>
        /// Identificador lógico ou código do leito dentro da unidade de saúde.
        /// </summary>
        public Guid BedId { get; init; }

        /// <summary>
        /// Identificador lógico ou código do leito dentro da unidade de saúde.
        /// </summary>
        public string BedNumber { get; init; } = string.Empty;

        /// <summary>
        /// Tipo do leito (ex.: UTI, Enfermaria, Pediátrico).
        /// </summary>
        public BedType Type { get; init; }

        /// <summary>
        /// Status atual do leito (ex.: Disponível, Ocupado, Manutenção).
        /// </summary>
        public BedStatus Status { get; init; }
    }
}