// Application/UseCases/Common/WeeklyWindowDto.cs


using SGHSS.Domain.Enums;

namespace SGHSS.Application.UseCases.Common
{
    /// <summary>
    /// Representa uma janela de tempo semanal recorrente informada pela camada de interface.
    /// </summary>
    /// <remarks>
    /// Este DTO é utilizado pela camada de Application para receber informações sobre janelas
    /// de disponibilidade semanais, sem expor diretamente o modelo de domínio.
    /// 
    /// Ele contém apenas dados estruturados, que serão posteriormente convertidos para a
    /// entidade <see cref="WeeklyWindow"/> durante a execução dos casos de uso.
    /// </remarks>
    public class WeeklyWindowDto
    {
        /// <summary>
        /// Dia da semana ao qual esta janela pertence.
        /// </summary>
        public WeekDay DayOfWeek { get; init; }

        /// <summary>
        /// Horário de início da janela de disponibilidade.
        /// </summary>
        public TimeOnly StartTime { get; init; }

        /// <summary>
        /// Horário de término da janela de disponibilidade.
        /// </summary>
        public TimeOnly EndTime { get; init; }
    }
}
