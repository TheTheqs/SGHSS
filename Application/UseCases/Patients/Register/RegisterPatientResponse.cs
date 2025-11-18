// Application/UseCases/Patients/Register/RegisterPatientResponse.cs

namespace SGHSS.Application.UseCases.Patients.Register
{
    /// <summary>
    /// Representa os dados de saída retornados após o registro bem-sucedido de um paciente.
    /// </summary>
    /// <remarks>
    /// Este DTO pode ser expandido para incluir informações adicionais, como dados de auditoria
    /// ou um resumo do paciente. No momento, expõe apenas o identificador gerado.
    /// </remarks>
    public sealed class RegisterPatientResponse
    {
        /// <summary>
        /// Identificador único do paciente recém-registrado.
        /// </summary>
        public Guid PatientId { get; }

        /// <summary>
        /// Cria uma nova instância de resposta para o caso de uso de registro de paciente.
        /// </summary>
        /// <param name="patientId">Identificador do paciente criado.</param>
        public RegisterPatientResponse(Guid patientId)
        {
            PatientId = patientId;
        }
    }
}
