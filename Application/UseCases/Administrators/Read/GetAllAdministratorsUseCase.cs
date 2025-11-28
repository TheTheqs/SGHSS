// Application/UseCases/Administrators/Read/GetAllAdministratorsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Administrators.Read
{
    /// <summary>
    /// Caso de uso responsável por listar todos os administradores do sistema
    /// em formato simplificado (ID e Nome).
    /// </summary>
    public sealed class GetAllAdministratorsUseCase
    {
        private readonly IAdministratorRepository _administratorRepository;

        public GetAllAdministratorsUseCase(IAdministratorRepository administratorRepository)
        {
            _administratorRepository = administratorRepository;
        }

        /// <summary>
        /// Recupera todos os administradores registrados,
        /// projetando-os em uma lista de <see cref="EntityDto"/>.
        /// </summary>
        /// <returns>
        /// Um <see cref="GetAllResponse"/> contendo a coleção de entidades simplificadas.
        /// </returns>
        public async Task<GetAllResponse> Handle()
        {
            var admins = await _administratorRepository.GetAllAsync();

            var items = admins
                .Select(a => new EntityDto(a.Id, a.Name))
                .ToList();

            return new GetAllResponse(items);
        }
    }
}
