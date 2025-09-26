using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Settings;
using App.Core.RepositoryServices;

namespace App.Services.Settings
{
    public class SettingService : ISettingService
    {
        private readonly IRepository<Setting> _settingRepository;

        public SettingService(IRepository<Setting> settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public async Task<string> GetAsync(string key, string defaultValue = null)
        {
            var s = (await _settingRepository.GetAllAsync(q => q.Where(x => x.Name == key))).FirstOrDefault();
            return s?.Value ?? defaultValue;
        }

        public async Task<T> GetAsync<T>(string key, T defaultValue = default)
        {
            var val = await GetAsync(key);
            if (val == null)
                return defaultValue;

            try
            {
                return (T)System.Convert.ChangeType(val, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        public async Task SetAsync(string key, string value)
        {
            var existing = (await _settingRepository.GetAllAsync(q => q.Where(x => x.Name == key))).FirstOrDefault();
            if (existing == null)
                await _settingRepository.InsertAsync(new Setting { Name = key, Value = value });
            else
            {
                existing.Value = value;
                await _settingRepository.UpdateAsync(existing);
            }
        }

        public async Task SetAsync<T>(string key, T value)
        {
            await SetAsync(key, value?.ToString());
        }

        public async Task<IDictionary<string, string>> GetAllAsync(string prefix = null)
        {
            var all = await _settingRepository.GetAllAsync(q =>
                string.IsNullOrWhiteSpace(prefix) ? q : q.Where(x => x.Name.StartsWith(prefix)));
            return all.ToDictionary(x => x.Name, x => x.Value);
        }

        public async Task<bool> HasKeyAsync(string key)
        {
            var any = await _settingRepository.GetAllAsync(q => q.Where(x => x.Name == key).Take(1));
            return any.Any();
        }

        public Task DeleteAsync(string key)
        {
            return _settingRepository.DeleteAsync(x => x.Name == key);
        }
    }
}
