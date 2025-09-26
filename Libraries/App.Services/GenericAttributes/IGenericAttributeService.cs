using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Common;

namespace App.Services.Common
{
    public interface IGenericAttributeService
    {
        Task<IList<GenericAttribute>> GetAttributesForEntityAsync(int entityId, string entityName);
        Task<GenericAttribute> GetAttributeAsync(int entityId, string entityName, string key);
        Task SetAttributeAsync<T>(int entityId, string entityName, string key, T value);
        Task DeleteAttributeAsync(int entityId, string entityName, string key);
    }
}
