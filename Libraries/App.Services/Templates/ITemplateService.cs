using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Common;
using App.Services.Common;

namespace App.Services.Templates
{
    public interface ITemplateService
    {
        Task<ServiceResult<Template>> GetAsync(int id);
        Task<ServiceResult<IReadOnlyList<Template>>> GetAllAsync(string type = null);
        Task<ServiceResult<Template>> CreateAsync(Template model);
        Task<ServiceResult> UpdateAsync(int id, Template model);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
