using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Common;
using App.Core.RepositoryServices;

namespace App.Services.Common
{
    public class GenericAttributeService : IGenericAttributeService
    {
        private readonly IRepository<GenericAttribute> _attributeRepository;

        public GenericAttributeService(IRepository<GenericAttribute> attributeRepository)
        {
            _attributeRepository = attributeRepository;
        }

        public async Task<IList<GenericAttribute>> GetAttributesForEntityAsync(int entityId, string entityName)
        {
            return (await _attributeRepository.GetAllAsync(q =>
                q.Where(a => a.EntityId == entityId && a.KeyGroup == entityName))).ToList();
        }

        public async Task<GenericAttribute> GetAttributeAsync(int entityId, string entityName, string key)
        {
            return (await _attributeRepository.GetAllAsync(q =>
                q.Where(a => a.EntityId == entityId && a.KeyGroup == entityName && a.Key == key)))
                .FirstOrDefault();
        }

        public async Task SetAttributeAsync<T>(int entityId, string entityName, string key, T value)
        {
            var existing = await GetAttributeAsync(entityId, entityName, key);
            var valueStr = value?.ToString();

            if (existing != null)
            {
                existing.Value = valueStr;
                await _attributeRepository.UpdateAsync(existing);
            }
            else
            {
                var attr = new GenericAttribute
                {
                    EntityId = entityId,
                    KeyGroup = entityName,
                    Key = key,
                    Value = valueStr
                };
                await _attributeRepository.InsertAsync(attr);
            }
        }

        public async Task DeleteAttributeAsync(int entityId, string entityName, string key)
        {
            var existing = await GetAttributeAsync(entityId, entityName, key);
            if (existing != null)
                await _attributeRepository.DeleteAsync(existing);
        }
    }
}
