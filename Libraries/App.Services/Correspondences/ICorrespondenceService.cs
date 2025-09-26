using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Correspondences;
using App.Services.Common;

namespace App.Services.Correspondences
{
    public interface ICorrespondenceService
    {
        Task<ServiceResult<Correspondence>> GetAsync(int id);
        Task<ServiceResult<IReadOnlyList<Correspondence>>> InboxAsync(int userId);
        Task<ServiceResult<IReadOnlyList<Correspondence>>> OutboxAsync(int userId);
        Task<ServiceResult<Correspondence>> SendAsync(Correspondence model);
        Task<ServiceResult> MarkAsAsync(int id, string status);
    }
}
