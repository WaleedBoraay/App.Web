using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Directory;
using App.Services.Common;

namespace App.Services.Event
{
    public interface IEventService
    {
        Task<ServiceResult<EventSchedule>> GetAsync(int id);
        Task<ServiceResult<IReadOnlyList<EventSchedule>>> GetRangeAsync(DateTime fromUtc, DateTime toUtc);
        Task<ServiceResult<EventSchedule>> CreateAsync(EventSchedule model);
        Task<ServiceResult> UpdateAsync(int id, EventSchedule model);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
