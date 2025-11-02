using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using App.Core.Domain.Directory;
using App.Core.RepositoryServices;
using App.Services.Common;
using App.Services.Localization;

namespace App.Services.Event
{
    public class EventService : BaseService, IEventService
    {
        private readonly IRepository<EventSchedule> _eventRepository;
        private readonly ILocalizationService _localizationService;

        public EventService(IRepository<EventSchedule> eventRepository, ILocalizationService localizationService)
        {
            _eventRepository = eventRepository;
            _localizationService = localizationService;
        }

        public async Task<ServiceResult<EventSchedule>> GetAsync(int id)
        {
            var e = await _eventRepository.GetByIdAsync(id);
            if (e == null) return ServiceResult<EventSchedule>.Failed(await _localizationService.GetResourceAsync("EventSchedule.Event.NotFound"));
            return ServiceResult<EventSchedule>.Success(e);
        }

        public async Task<ServiceResult<IReadOnlyList<EventSchedule>>> GetRangeAsync(DateTime fromUtc, DateTime toUtc)
        {
            var list = await _eventRepository.GetAllAsync(q => q.Where(x => x.StartDateUtc <= toUtc && x.EndDateUtc >= fromUtc));
            return ServiceResult<IReadOnlyList<EventSchedule>>.Success(list.ToList());
        }

        public async Task<ServiceResult<EventSchedule>> CreateAsync(EventSchedule model)
        {
            if (model == null) return ServiceResult<EventSchedule>.Failed(await _localizationService.GetResourceAsync("Errors.NullModel"));
            if (model.EndDateUtc < model.StartDateUtc) return ServiceResult<EventSchedule>.Failed(await _localizationService.GetResourceAsync("EventSchedule.InvalidRange", "Invalid range"));
            await _eventRepository.InsertAsync(model);
            return ServiceResult<EventSchedule>.Success(model);
        }

        public async Task<ServiceResult> UpdateAsync(int id, EventSchedule model)
        {
            var e = await _eventRepository.GetByIdAsync(id);
            if (e == null) return ServiceResult.Failed(await _localizationService.GetResourceAsync("EventSchedule.Event.NotFound"));
            e.Title = model.Title;
            e.Description = model.Description;
            e.StartDateUtc = model.StartDateUtc;
            e.EndDateUtc = model.EndDateUtc;
            e.IsAllDay = model.IsAllDay;
            e.Location = model.Location;
            e.EventType = model.EventType;
            e.Color = model.Color;
            e.NotifyUsers = model.NotifyUsers;
            e.NotificationSentAt = model.NotificationSentAt;
            e.CreatedByUserId = model.CreatedByUserId;
            await _eventRepository.UpdateAsync(e);
            return ServiceResult.SuccessResult();
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var e = await _eventRepository.GetByIdAsync(id);
            if (e == null) return ServiceResult.Failed(await _localizationService.GetResourceAsync("EventSchedule.Event.NotFound"));
            await _eventRepository.DeleteAsync(e);
            return ServiceResult.SuccessResult();
        }
    }
}
