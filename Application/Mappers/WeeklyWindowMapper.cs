// Application/Mappers/WeeklyWindowMapper.cs

namespace SGHSS.Application.Mappers
{
    public static class WeeklyWindowMapper
    {
        /// <summary>
        /// Converte WeeklyWindowDto em WeeklyWindow Value Object
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static Domain.Models.WeeklyWindow ToDomain(this UseCases.Common.WeeklyWindowDto dto)
        {
            return new Domain.Models.WeeklyWindow
            {
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };
        }
    }
}
