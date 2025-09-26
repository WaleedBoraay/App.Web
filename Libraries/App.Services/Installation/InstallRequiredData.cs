using App.Core.Domain.Common;
using App.Core.Domain.Registrations;
using App.Core.Domain.Security;
using App.Core.Domain.Settings;
using App.Core.Domain.Users;
using App.Core.RepositoryServices;
using App.Services.Security;
using App.Services.Settings;
using App.Services.Templates;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Installation
{
    /// <summary>
    /// Seeds required data that must exist before the app can run.
    /// </summary>
    public partial class InstallRequiredData
    {
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<AppUser> _userRepository;
        private readonly IRepository<UserRole> _userRoleRepository;
        private readonly IRepository<WorkflowStep> _workflowStepRepository;
        private readonly ITemplateService _templateService;
        private readonly ISettingService _settingService;
        private readonly IEncryptionService _encryptionService;

        public InstallRequiredData(
            IRepository<Role> roleRepository,
            IRepository<AppUser> userRepository,
            IRepository<UserRole> userRoleRepository,
            IRepository<WorkflowStep> workflowStepRepository,
            ITemplateService templateService,
            ISettingService settingService,
            IEncryptionService encryptionService)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _workflowStepRepository = workflowStepRepository;
            _templateService = templateService;
            _settingService = settingService;
            _encryptionService = encryptionService;
        }

        public async Task InstallAsync()
        {
            await SeedRolesAsync();
            await SeedAdminUserAsync();
            await SeedWorkflowStepsAsync();
            await SeedDefaultSettingsAsync();
            await SeedNotificationTemplatesAsync();
        }

        #region Roles
        private async Task SeedRolesAsync()
        {
            var required = new[] { "Super Admin", "Admin", "Maker", "Checker" };
            foreach (var roleName in required)
            {
                var exists = (await _roleRepository.GetAllAsync(q => q.Where(r => r.Name == roleName))).FirstOrDefault();
                if (exists == null)
                {
                    var role = new Role
                    {
                        Name = roleName,
                        Description = roleName,
                        SystemName = roleName,
                        IsActive = true,
                        CreatedOnUtc = DateTime.UtcNow,
                        IsSystemRole = true,
                    };
                    await _roleRepository.InsertAsync(role);
                }
            }
        }
        #endregion

        #region Admin user
        private async Task SeedAdminUserAsync()
        {
            var admin = (await _userRepository.GetAllAsync(q => q.Where(u => u.Username == "admin"))).FirstOrDefault();
            if (admin == null)
            {
                var salt = _encryptionService.CreateSaltKey(16);
                var passwordFormat = PasswordFormat.Hashed;
                admin = new AppUser
                {
                    Username = "admin",
                    Email = "admin@system.local",
                    PasswordFormatId = (int)passwordFormat,
                    PasswordSalt = salt,
                    PasswordFormat = passwordFormat,
                    PasswordHash = _encryptionService.CreatePasswordHash("Admin@123", salt, passwordFormat),
                    IsActive = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    FailedLoginAttempts = 0
                };
                await _userRepository.InsertAsync(admin);
            }

            var adminRole = (await _roleRepository.GetAllAsync(q => q.Where(r => r.Name == "Admin"))).FirstOrDefault();
            if (adminRole != null)
            {
                var mappingExists = (await _userRoleRepository.GetAllAsync(q => q.Where(m => m.UserId == admin.Id && m.RoleId == adminRole.Id))).Any();
                if (!mappingExists)
                {
                    await _userRoleRepository.InsertAsync(new UserRole
                    {
                        UserId = admin.Id,
                        RoleId = adminRole.Id,
                    });
                }
            }
        }
        #endregion

        #region Workflow
        private async Task SeedWorkflowStepsAsync()
        {
            var draft = await EnsureWorkflowStepAsync("Draft", "Maker");
            var submitted = await EnsureWorkflowStepAsync("Submitted", "Checker");
            var underReview = await EnsureWorkflowStepAsync("Under Review", "Regulator");
            var approved = await EnsureWorkflowStepAsync("Approved", "Regulator");
            var rejected = await EnsureWorkflowStepAsync("Rejected", "Regulator");
            var returned = await EnsureWorkflowStepAsync("Returned for Edit", "Checker,Regulator");

            await LinkNextAsync(draft, submitted);
            await LinkNextAsync(submitted, underReview);
            await LinkNextAsync(underReview, approved);
        }

        private async Task<WorkflowStep> EnsureWorkflowStepAsync(string name, string roleAllowed)
        {
            var step = (await _workflowStepRepository.GetAllAsync(q => q.Where(s => s.Name == name))).FirstOrDefault();
            if (step == null)
            {
                step = new WorkflowStep
                {
                    Name = name,
                    //RoleAllowed = roleAllowed,
                    NextStepId = null
                };
                await _workflowStepRepository.InsertAsync(step);
            }
            else if (step.RoleAllowed.Name != roleAllowed)
            {
                step.RoleAllowed.Name = roleAllowed;
                await _workflowStepRepository.UpdateAsync(step);
            }
            return step;
        }

        private async Task LinkNextAsync(WorkflowStep from, WorkflowStep to)
        {
            if (from == null || to == null) return;
            if (from.NextStepId != to.Id)
            {
                from.NextStepId = to.Id;
                await _workflowStepRepository.UpdateAsync(from);
            }
        }
        #endregion

        #region Default settings
        private async Task SeedDefaultSettingsAsync()
        {
            await _settingService.SetAsync("Site.Name", "Supervision System");
            await _settingService.SetAsync("Site.TimeZone", "Africa/Cairo");
            await _settingService.SetAsync("Site.DefaultLanguage", "en-US");
            await _settingService.SetAsync("Security.Password.MinLength", 8);
            await _settingService.SetAsync("Security.Password.RequireDigit", true);
            await _settingService.SetAsync("Security.Password.RequireUppercase", true);
            await _settingService.SetAsync("Security.Lockout.MaxFailedAccessAttempts", 5);
            await _settingService.SetAsync("Security.Lockout.DefaultLockoutMinutes", 15);
        }
        #endregion

        #region Notification templates
        private async Task SeedNotificationTemplatesAsync()
        {
            await _templateService.CreateAsync(new Template
            {
                Name = "Notification.RegistrationSubmitted",
                TemplateType = "Email",
                Content = "Registration submitted successfully.",
                CreatedByUserId = 0,
                CreatedOnUtc = DateTime.UtcNow
            });
        }
        #endregion
    }
}
