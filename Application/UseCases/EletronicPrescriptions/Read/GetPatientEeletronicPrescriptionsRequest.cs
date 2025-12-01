// Application/UseCases/EletronicPrescriptions/GetPatientEletronicPrescriptionsRequest.cs

namespace SGHSS.Application.UseCases.EletronicPrescriptions.Read
{
    /// <summary>
    /// Representa os dados necessários para consultar todas as
    /// prescrições eletrônicas associadas a um paciente.
    /// </summary>
    /// <remarks>
    /// Este request utiliza apenas o identificador do paciente.
    /// Regras de autorização e verificação de vínculo entre o usuário
    /// autenticado e o paciente devem ser tratadas na camada de controle.
    /// </remarks>
    public sealed class GetPatientEletronicPrescriptionsRequest
    {
        /// <summary>
        /// Identificador do paciente cujas prescrições eletrônicas
        /// deverão ser consultadas.
        /// </summary>
        public Guid PatientId { get; init; }
    }
}
