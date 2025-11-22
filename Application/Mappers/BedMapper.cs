// Application/Mappers/BedMapper.cs

namespace SGHSS.Application.Mappers
{
    /// <summary>
    /// Classe responsável por converter objetos <see cref="BedDto"/>
    /// em suas respectivas entidades de domínio (<see cref="Domain.Models.Bed"/>).
    /// </summary>
    /// <remarks>
    /// O mapper centraliza a lógica de transformação entre camadas,
    /// garantindo isolamento e evitando duplicação de código.
    /// É utilizado em casos de uso que manipulam leitos, como adição,
    /// remoção e atualização de status.
    /// </remarks>
    public static class BedMapper
    {
        /// <summary>
        /// Converte um <see cref="BedDto"/> para a entidade de domínio <see cref="Domain.Models.Bed"/>.
        /// </summary>
        /// <param name="dto">Objeto transportado na camada de Application contendo dados do leito.</param>
        /// <returns>Instância da entidade <see cref="Domain.Models.Bed"/> mapeada a partir do DTO.</returns>
        public static Domain.Models.Bed ToDomain(this Application.UseCases.Common.BedDto dto)
        {
            // Mapeamento direto de propriedades simples do DTO para a entidade de domínio.
            return new Domain.Models.Bed
            {
                BedNumber = dto.BedNumber,
                Type = dto.Type,
                Status = dto.Status
            };
        }
    }
}
