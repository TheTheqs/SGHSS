// Application/UseCases/Patients/Read/ConsultMedicalRecord/ConsultMedicalRecordResponse.cs

using SGHSS.Application.UseCases.Common;

namespace SGHSS.Application.UseCases.Patients.ConsultMedicalRecord
{
    /// <summary>
    /// Representa os dados retornados pela consulta de prontuário de um paciente.
    /// </summary>
    /// <remarks>
    /// Este DTO agrega o identificador do paciente e um objeto de prontuário
    /// contendo suas informações principais e o histórico de atualizações.
    /// </remarks>
    public sealed class ConsultMedicalRecordResponse
    {
        /// <summary>
        /// Identificador único do paciente ao qual o prontuário consultado pertence.
        /// </summary>
        public Guid PatientId { get; init; }

        /// <summary>
        /// Prontuário do paciente, incluindo metadados e histórico de atualizações.
        /// </summary>
        public MedicalRecordDto MedicalRecord { get; init; } = null!;
    }
}
