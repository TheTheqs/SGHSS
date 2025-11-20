// Application/Mappers/ConsentMapper.cs

namespace SGHSS.Application.Mappers
{
    public static class ConsentMapper
    {
        /// <summary>
        /// Converte ConsentDto em Consent
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static Domain.Models.Consent ToDomain(this UseCases.Common.ConsentDto dto)
        {
            return new Domain.Models.Consent
            {
                Scope = dto.Scope,
                TermVersion = dto.TermVersion,
                Channel = dto.Channel,
                ConsentDate = dto.ConsentDate,
                RevocationDate = dto.RevocationDate,
                TermHash = new Domain.ValueObjects.HashDigest(dto.TermHash)
            };
        }
    }
}
