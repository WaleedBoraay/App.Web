using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Directory;
using App.Services.Common;

namespace App.Services.Calendar
{
    public interface ICalendarService
    {
        Task<ServiceResult<CalendarEvent>> GetAsync(int id);
        Task<ServiceResult<IReadOnlyList<CalendarEvent>>> GetRangeAsync(DateTime fromUtc, DateTime toUtc);
        Task<ServiceResult<CalendarEvent>> CreateAsync(CalendarEvent model);
        Task<ServiceResult> UpdateAsync(int id, CalendarEvent model);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
