// Application/UseCases/Administrators/Update/HospitalizePatientResponse.cs

namespace SGHSS.Application.UseCases.Administrators.Update
{
    /// <summary>
    /// Representa o resultado da operação de hospitalização de um paciente.
    /// </summary>
    /// <remarks>
    /// Esta resposta contém apenas os dados essenciais gerados durante o processo
    /// de internação, permitindo que a camada superior (controllers, serviços externos
    /// ou UI) tenha acesso às informações-chave sem expor todo o agregado do paciente.
    /// </remarks>
    public sealed class HospitalizePatientResponse
    {
        /// <summary>
        /// Identificador único da internação criada.
        /// </summary>
        public Guid HospitalizationId { get; init; }

        /// <summary>
        /// Identificador do paciente vinculado à internação.
        /// </summary>
        public Guid PatientId { get; init; }

        /// <summary>
        /// Identificador da cama ocupada durante a internação.
        /// </summary>
        public Guid BedId { get; init; }

        /// <summary>
        /// Data e hora de admissão registrada no sistema.
        /// </summary>
        public DateTimeOffset AdmissionDate { get; init; }

        /// <summary>
        /// Motivo que levou à internação.
        /// </summary>
        public string Reason { get; init; } = string.Empty;

        /// <summary>
        /// Cria uma nova instância da resposta de hospitalização.
        /// </summary>
        public HospitalizePatientResponse(
            Guid hospitalizationId,
            Guid patientId,
            Guid bedId,
            DateTimeOffset admissionDate,
            string reason)
        {
            HospitalizationId = hospitalizationId;
            PatientId = patientId;
            BedId = bedId;
            AdmissionDate = admissionDate;
            Reason = reason;
        }
    }
}
