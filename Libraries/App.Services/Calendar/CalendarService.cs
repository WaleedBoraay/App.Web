using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using App.Core.Domain.Directory;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;

namespace App.Services.Calendar
{
    public class CalendarService : BaseService, ICalendarService
    {
        private readonly IRepository<CalendarEvent> _calendarEventRepository;
        private readonly ILocalizationService _localizationService;

        public CalendarService(IRepository<CalendarEvent> calendarEventRepository, ILocalizationService localizationService)
        {
            _calendarEventRepository = calendarEventRepository;
            _localizationService = localizationService;
        }

        public async Task<ServiceResult<CalendarEvent>> GetAsync(int id)
        {
            var e = await _calendarEventRepository.GetByIdAsync(id);
            if (e == null) return ServiceResult<CalendarEvent>.Failed(await _localizationService.GetResourceAsync("Calendar.Event.NotFound"));
            return ServiceResult<CalendarEvent>.Success(e);
        }

        public async Task<ServiceResult<IReadOnlyList<CalendarEvent>>> GetRangeAsync(DateTime fromUtc, DateTime toUtc)
        {
            var list = await _calendarEventRepository.GetAllAsync(q => q.Where(x => x.StartDateUtc <= toUtc && x.EndDateUtc >= fromUtc));
            return ServiceResult<IReadOnlyList<CalendarEvent>>.Success(list.ToList());
        }

        public async Task<ServiceResult<CalendarEvent>> CreateAsync(CalendarEvent model)
        {
            if (model == null) return ServiceResult<CalendarEvent>.Failed(await _localizationService.GetResourceAsync("Errors.NullModel"));
            if (model.EndDateUtc < model.StartDateUtc) return ServiceResult<CalendarEvent>.Failed(await _localizationService.GetResourceAsync("Calendar.InvalidRange", "Invalid range"));
            await _calendarEventRepository.InsertAsync(model);
            return ServiceResult<CalendarEvent>.Success(model);
        }

        public async Task<ServiceResult> UpdateAsync(int id, CalendarEvent model)
        {
            var e = await _calendarEventRepository.GetByIdAsync(id);
            if (e == null) return ServiceResult.Failed(await _localizationService.GetResourceAsync("Calendar.Event.NotFound"));
            e.Title = model.Title;
            e.Description = model.Description;
            e.StartDateUtc = model.StartDateUtc;
            e.EndDateUtc = model.EndDateUtc;
            e.IsHoliday = model.IsHoliday;
            e.EventType = model.EventType;
            await _calendarEventRepository.UpdateAsync(e);
            return ServiceResult.SuccessResult();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var e = await _calendarEventRepository.GetByIdAsync(id);
            if (e == null) return ServiceResult.Failed(await _localizationService.GetResourceAsync("Calendar.Event.NotFound"));
            await _calendarEventRepository.DeleteAsync(e);
            return ServiceResult.SuccessResult();
        }
    }
}
