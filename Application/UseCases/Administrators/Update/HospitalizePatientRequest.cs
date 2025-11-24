// Application/UseCases/Administrators/Update/HospitalizePatientRequest.cs

using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Administrators.Update
{
    /// <summary>
    /// Dados necessários para iniciar uma internação de um paciente.
    /// </summary>
    /// <remarks>
    /// Este objeto representa o comando de entrada para o caso de uso de hospitalização.
    /// Ele carrega apenas informações essenciais — identificadores e motivo — deixando
    /// para o caso de uso a responsabilidade de carregar entidades, validar regras de
    /// negócio e executar as transições de estado.
    /// 
    /// Os atributos obrigatórios são o identificador do paciente, o identificador da cama
    /// a ser ocupada e a razão da internação. A validação profunda dessas informações
    /// ocorre na camada de Application durante a execução do caso de uso.
    /// </remarks>
    public sealed class HospitalizePatientRequest
    {
        /// <summary>
        /// Identificador único do paciente que será hospitalizado.
        /// </summary>
        public Guid PatientId { get; init; }

        /// <summary>
        /// Identificador único da cama que será vinculada à internação.
        /// </summary>
        /// <remarks>
        /// A cama deve estar em estado <see cref="BedStatus.Available"/>.
        /// Qualquer outro estado resultará em erro durante o caso de uso.
        /// </remarks>
        public Guid BedId { get; init; }

        /// <summary>
        /// Motivo clínico ou administrativo que justifica a internação.
        /// </summary>
        /// <remarks>
        /// Este campo será gravado diretamente no modelo <c>Hospitalization</c>
        /// e armazenado no prontuário do paciente.
        /// </remarks>
        public string Reason { get; init; } = string.Empty;
    }
}
