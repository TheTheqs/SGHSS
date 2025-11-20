// Application/Mappers/AddressMapper.cs

namespace SGHSS.Application.Mappers
{
    public static class AddressMapper
    {
        /// <summary>
        /// Converte AddressDto em Address Value Object
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static Domain.ValueObjects.Address ToDomain(this UseCases.Common.AddressDto dto)
        {
            return new Domain.ValueObjects.Address(
                street: dto.Street,
                number: dto.Number,
                city: dto.City,
                state: dto.State,
                cep: dto.Cep,
                district: dto.District,
                complement: dto.Complement,
                country: dto.Country
            );
        }
    }
}
