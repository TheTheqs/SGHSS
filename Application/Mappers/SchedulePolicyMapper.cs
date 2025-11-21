// Application/Mappers/SchedulePolicyMapper.cs

namespace SGHSS.Application.Mappers
{
    public static class SchedulePolicyMapper
    {
        /// <summary>
        /// Converte SchedulePolicyDto em SchedulePolicy
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static Domain.Models.SchedulePolicy ToDomain(this UseCases.Common.SchedulePolicyDto dto)
        {
            return new Domain.Models.SchedulePolicy
            {
                DurationInMinutes = dto.DurationInMinutes,
                TimeZone = new Domain.ValueObjects.TimeZone(dto.TimeZone),
                WeeklyWindows = dto.WeeklyWindows
                    .Select(wwDto => wwDto.ToDomain())
                    .ToList()
            };
        }
    }
}
