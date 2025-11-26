// Application/UseCases/HomeCares/Read/GetPatientHomeCaresResponse.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.HomeCares.Read
{
    /// <summary>
    /// Representa a resposta contendo a lista de atendimentos de home care
    /// associados a um paciente específico.
    /// </summary>
    public sealed class GetPatientHomeCaresResponse
    {
        /// <summary>
        /// Coleção de registros de home care encontrados para o paciente.
        /// </summary>
        public IReadOnlyCollection<HomeCareDto> HomeCares { get; init; } =
            Array.Empty<HomeCareDto>();
    }
}
