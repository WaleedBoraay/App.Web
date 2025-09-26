using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.Settings
{
    public interface ISettingService
    {
        Task<string> GetAsync(string key, string defaultValue = null);
        Task<T> GetAsync<T>(string key, T defaultValue = default);
        Task SetAsync(string key, string value);
        Task SetAsync<T>(string key, T value);
        Task<IDictionary<string, string>> GetAllAsync(string prefix = null);
        Task<bool> HasKeyAsync(string key);
        Task DeleteAsync(string key);
    }
}
