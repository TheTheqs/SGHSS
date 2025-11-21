// Application/UseCases/Common/SchedulePolicyDto.cs


namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa os dados estruturados de uma política de agendamento,
    /// recebidos pela camada de interface.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado pela camada de Application para configurar a política de agenda
    /// de um profissional, sem expor diretamente o modelo de domínio <see cref="SchedulePolicy"/>.
    /// 
    /// Os dados aqui presentes serão convertidos para a entidade de domínio durante a execução
    /// dos casos de uso, incluindo a criação do value object <c>TimeZone</c> e das
    /// janelas semanais associadas.
    /// </remarks>
    public sealed class SchedulePolicyDto
    {
        /// <summary>
        /// Duração padrão, em minutos, de cada atendimento/agendamento.
        /// </summary>
        public int DurationInMinutes { get; init; }

        /// <summary>
        /// Fuso horário utilizado pela política de agendamento.
        /// </summary>
        /// <remarks>
        /// Deve ser informado como string em formato aceito pelo value object <c>TimeZone</c>,
        /// por exemplo: "Etc/UTC", "UTC-03:00" ou "America/Sao_Paulo".
        /// </remarks>
        public string TimeZone { get; init; } = string.Empty;

        /// <summary>
        /// Coleção de janelas semanais que definem a disponibilidade recorrente
        /// associada a esta política de agendamento.
        /// </summary>
        public ICollection<WeeklyWindowDto> WeeklyWindows { get; init; } = new List<WeeklyWindowDto>();
    }
}
