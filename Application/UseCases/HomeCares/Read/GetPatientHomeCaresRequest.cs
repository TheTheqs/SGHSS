// Application/UseCases/HomeCares/Read/GetPatientHomeCaresRequest.cs

namespace SGHSS.Application.UseCases.HomeCares.Read
{
    /// <summary>
    /// Representa a solicitação para consulta dos atendimentos
    /// de home care associados a um determinado paciente.
    /// </summary>
    public sealed class GetPatientHomeCaresRequest
    {
        /// <summary>
        /// Identificador único do paciente cujos registros de home care
        /// devem ser consultados.
        /// </summary>
        public Guid PatientId { get; init; }
    }
}
