// Application/UseCases/Patients/Update/UpdateMedicalRecordRequest.cs

namespace SGHSS.Application.UseCases.Patients.Update
{
    /// <summary>
    /// Representa os dados necessários para registrar uma nova atualização
    /// no prontuário médico de um paciente.
    /// </summary>
    /// <remarks>
    /// Este request contém apenas identificadores de entidades já existentes
    /// (paciente, profissional, unidade de saúde e, opcionalmente, consulta),
    /// além da descrição clínica da atualização. 
    /// O momento da atualização (UpdateDate) é definido pelo caso de uso,
    /// garantindo padronização (ex.: uso de UTC) em toda a aplicação.
    /// </remarks>
    public sealed class UpdateMedicalRecordRequest
    {
        /// <summary>
        /// Identificador do paciente cujo prontuário será atualizado.
        /// </summary>
        public Guid PatientId { get; init; }

        /// <summary>
        /// Identificador do profissional responsável pela atualização.
        /// </summary>
        public Guid ProfessionalId { get; init; }

        /// <summary>
        /// Identificador da unidade de saúde em que a atualização foi realizada.
        /// </summary>
        public Guid HealthUnitId { get; init; }

        /// <summary>
        /// Identificador da consulta associada à atualização, quando aplicável.
        /// </summary>
        /// <remarks>
        /// Este campo é opcional e deve ser utilizado quando a atualização de prontuário
        /// estiver diretamente vinculada a um atendimento ou consulta específica.
        /// </remarks>
        public Guid? AppointmentId { get; init; }

        /// <summary>
        /// Texto descritivo contendo a anotação ou atualização clínica
        /// a ser registrada no prontuário do paciente.
        /// </summary>
        public string Description { get; init; } = null!;
    }
}
