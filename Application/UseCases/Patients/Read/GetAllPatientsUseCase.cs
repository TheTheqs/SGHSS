// Application/UseCases/Patients/Read/GetAllPatientsUseCase.cs

using SGHSS.Application.Interfaces.Repositories;
using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Patients.Read
{
    /// <summary>
    /// Caso de uso responsável por listar todos os pacientes do sistema
    /// em formato simplificado (ID e Nome).
    /// </summary>
    public sealed class GetAllPatientsUseCase
    {
        private readonly IPatientRepository _patientRepository;

        public GetAllPatientsUseCase(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        /// <summary>
        /// Recupera todos os pacientes registrados,
        /// projetando-os em uma lista de <see cref="EntityDto"/>.
        /// </summary>
        /// <returns>
        /// Um <see cref="GetAllResponse"/> contendo a coleção de entidades simplificadas.
        /// </returns>
        public async Task<GetAllResponse> Handle()
        {
            var patients = await _patientRepository.GetAllAsync();

            var items = patients
                .Select(p => new EntityDto(p.Id, p.Name))
                .ToList();

            return new GetAllResponse(items);
        }
    }
}
