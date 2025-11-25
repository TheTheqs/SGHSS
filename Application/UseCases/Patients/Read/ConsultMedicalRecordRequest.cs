// Application/UseCases/Patients/Read/ConsultMedicalRecord/ConsultMedicalRecordRequest.cs

namespace SGHSS.Application.UseCases.Patients.ConsultMedicalRecord
{
    /// <summary>
    /// Representa os dados de entrada necessários para consultar o prontuário
    /// eletrônico de um paciente específico.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado pela camada de Application para receber o identificador
    /// do paciente a partir da camada de interface (por exemplo, uma API) e
    /// orquestrar o caso de uso de consulta ao prontuário, sem expor diretamente
    /// as entidades de domínio.
    /// </remarks>
    public sealed class ConsultMedicalRecordRequest
    {
        /// <summary>
        /// Identificador único do paciente cujo prontuário será consultado.
        /// </summary>
        public Guid PatientId { get; init; }
    }
}
