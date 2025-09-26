using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Reports;
using App.Services.Common;

namespace App.Services.Reports
{
    public interface IReportService
    {
        Task<ServiceResult<Report>> GetAsync(int id);
        Task<ServiceResult<IReadOnlyList<Report>>> GetAllAsync(string type = null);
        Task<ServiceResult<Report>> CreateAsync(Report model);
        Task<ServiceResult> UpdateAsync(int id, Report model);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
