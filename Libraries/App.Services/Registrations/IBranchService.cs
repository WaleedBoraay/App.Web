using App.Core.Domain.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Registrations
{
    public interface IBranchService
    {
        Task<Branch> GetByIdAsync(int id);
        Task<IList<Branch>> GetByRegistrationIdAsync(int insterutionId);
        Task<IList<Branch>> GetAllAsync();

        Task<Branch> InsertAsync(Branch branch);
        Task<Branch> UpdateAsync(Branch branch);
        Task DeleteAsync(int id);
    }
}
