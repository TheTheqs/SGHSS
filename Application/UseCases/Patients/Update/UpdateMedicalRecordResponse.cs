// Application/UseCases/Patients/Update/UpdateMedicalRecordResponse.cs

namespace SGHSS.Application.UseCases.Patients.Update
{
    /// <summary>
    /// Representa o resultado da operação de atualização de prontuário médico.
    /// </summary>
    /// <remarks>
    /// Este response expõe os identificadores principais e metadados
    /// da atualização recém-criada, permitindo que camadas superiores
    /// exibam ou registrem o evento de forma rastreável.
    /// </remarks>
    public sealed class UpdateMedicalRecordResponse
    {
        /// <summary>
        /// Identificador do prontuário médico que recebeu a atualização.
        /// </summary>
        public Guid MedicalRecordId { get; }

        /// <summary>
        /// Identificador da atualização de prontuário criada.
        /// </summary>
        public Guid MedicalRecordUpdateId { get; }

        /// <summary>
        /// Data e hora em que a atualização foi registrada.
        /// </summary>
        /// <remarks>
        /// A data é definida pelo caso de uso (normalmente em UTC) no momento da persistência.
        /// </remarks>
        public DateTime UpdateDate { get; }

        /// <summary>
        /// Descrição clínica registrada na atualização.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Cria uma nova instância do response de atualização de prontuário.
        /// </summary>
        /// <param name="medicalRecordId">Id do prontuário atualizado.</param>
        /// <param name="medicalRecordUpdateId">Id da atualização criada.</param>
        /// <param name="updateDate">Data/hora de registro da atualização.</param>
        /// <param name="description">Descrição clínica registrada.</param>
        public UpdateMedicalRecordResponse(
            Guid medicalRecordId,
            Guid medicalRecordUpdateId,
            DateTime updateDate,
            string description)
        {
            MedicalRecordId = medicalRecordId;
            MedicalRecordUpdateId = medicalRecordUpdateId;
            UpdateDate = updateDate;
            Description = description;
        }
    }
}
