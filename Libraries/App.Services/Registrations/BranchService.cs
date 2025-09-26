using App.Core.Domain.Registrations;
using App.Core.RepositoryServices;
using App.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Services.Registrations
{
    public class BranchService : IBranchService
    {
        private readonly IRepository<Branch> _branchRepository;
        private readonly ILocalizationService _localizationService;

        public BranchService(
            IRepository<Branch> branchRepository,
            ILocalizationService localizationService)
        {
            _branchRepository = branchRepository;
            _localizationService = localizationService;
        }

        public async Task<Branch> GetByIdAsync(int id)
            => await _branchRepository.GetByIdAsync(id);

        public async Task<IList<Branch>> GetByRegistrationIdAsync(int insterutionId)
            => await _branchRepository.GetAllAsync(q => q.Where(b => b.InstitutionId == insterutionId));

        public async Task<IList<Branch>> GetAllAsync()
        {
            var branch = await _branchRepository.GetAllAsync(query =>
                query.OrderBy(b => b.Name)
            );
            return branch;
        }


        public async Task<Branch> InsertAsync(Branch branch)
        {
            if (branch == null)
                throw new ArgumentNullException(nameof(branch), await _localizationService.GetResourceAsync("Branch.Insert.Null"));

            branch.CreatedOnUtc = DateTime.UtcNow;
            await _branchRepository.InsertAsync(branch);

            return branch;
        }

        public async Task<Branch> UpdateAsync(Branch branch)
        {
            if (branch == null)
                throw new ArgumentNullException(nameof(branch), await _localizationService.GetResourceAsync("Branch.Update.Null"));

            branch.UpdatedOnUtc = DateTime.UtcNow;
            await _branchRepository.UpdateAsync(branch);

            return branch;
        }

        public async Task DeleteAsync(int id)
        {
            var branch = await _branchRepository.GetByIdAsync(id);
            if (branch == null)
                throw new ArgumentNullException(nameof(branch), await _localizationService.GetResourceAsync("Branch.NotFound"));

            await _branchRepository.DeleteAsync(branch);
        }
    }
}
