// Application/UseCases/Professionals/Read/GetAllProfessionalsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Professionals.Read
{
    /// <summary>
    /// Caso de uso responsável por listar todos os profissionais de saúde
    /// em formato simplificado (ID e Nome).
    /// </summary>
    public sealed class GetAllProfessionalsUseCase
    {
        private readonly IProfessionalRepository _professionalRepository;

        public GetAllProfessionalsUseCase(IProfessionalRepository professionalRepository)
        {
            _professionalRepository = professionalRepository;
        }

        /// <summary>
        /// Recupera todos os profissionais registrados,
        /// projetando-os em uma lista de <see cref="EntityDto"/>.
        /// </summary>
        /// <returns>
        /// Um <see cref="GetAllResponse"/> contendo a coleção de entidades simplificadas.
        /// </returns>
        public async Task<GetAllResponse> Handle()
        {
            var professionals = await _professionalRepository.GetAllAsync();

            var items = professionals
                .Select(p => new EntityDto(p.Id, p.Name))
                .ToList();

            return new GetAllResponse(items);
        }
    }
}
