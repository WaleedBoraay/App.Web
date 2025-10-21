using App.Core;
using App.Core.AppSettingsConfig;
using App.Core.Configuration;
using App.Core.Domain.Security;
using App.Core.Domain.Settings;
using App.Core.Domain.Users;
using App.Core.Infrastructure;
using App.Core.MigratorServices;
using App.Core.RepositoryServices;
using App.Data.EngineServices;
using App.Services.Installation;
using App.Services.Security;
using App.Web.Models.Install;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class InstallController : Controller
    {
        #region Fields
        protected readonly AppSettings _appSettings;
        protected readonly IAppFileProvider _fileProvider;
        private readonly IInstallationService _installationService;
        private readonly IAppDataProvider _dataProvider;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IRepository<Setting> _settingRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IPermissionService _permissionService;
        private readonly IPermissionSyncService _permissionSyncService;
        #endregion

        #region Ctor
        public InstallController(
            AppSettings appSettings,
            IAppFileProvider fileProvider,
            IInstallationService installationService,
            IAppDataProvider dataProvider,
            IHostApplicationLifetime lifetime,
            IRepository<Setting> settingRepository,
            IEncryptionService encryptionService,
            IPermissionService permissionService,
            IPermissionSyncService permissionSyncService
            )
        {
            _appSettings = appSettings;
            _fileProvider = fileProvider;
            _installationService = installationService;
            _dataProvider = dataProvider;
            _lifetime = lifetime;
            _settingRepository = settingRepository;
            _encryptionService = encryptionService;
            _permissionService = permissionService;
            _permissionSyncService = permissionSyncService;
        }
        #endregion

        #region Utilities (kept close to old logic)
        private static string BuildSqlServerConnectionString(InstallModel model)
        {
            // 1) Raw string takes priority (old behavior)
            if (model.ConnectionStringRaw && !string.IsNullOrWhiteSpace(model.ConnectionString))
                return model.ConnectionString!;

            // 2) Build from fields
            if (string.IsNullOrWhiteSpace(model.ServerName) || string.IsNullOrWhiteSpace(model.DatabaseName))
                return string.Empty;

            // Prefer SQL auth if username/password provided, otherwise Windows auth
            if (!string.IsNullOrWhiteSpace(model.Username) && !string.IsNullOrWhiteSpace(model.Password))
            {
                return $"Server={model.ServerName};Database={model.DatabaseName};User Id={model.Username};Password={model.Password};TrustServerCertificate=True;MultipleActiveResultSets=True";
            }

            return $"Server={model.ServerName};Database={model.DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
        }

        protected virtual InstallModel PrepareAvailableDataProviders(InstallModel model)
        {
            var providers = Enum.GetValues(typeof(DataProviderType))
                .Cast<DataProviderType>()
                .Select(v => new SelectListItem { Value = v.ToString(), Text = v.ToString() })
                .ToList();

            ViewBag.AvailableDataProviders = providers;
            return model;
        }

        protected virtual InstallModel PrepareLanguageList(InstallModel model)
        {
            model.AvailableLanguages.Add(
                new SelectListItem
                {
                    Value = Url.Action("ChangeLanguage", "Install", new { language = "en" })!,
                    Text = "English",
                    Selected = true
                });
            model.AvailableLanguages.Add(
            new SelectListItem
            {
                Value = Url.Action("ChangeLanguage", "Install", new { language = "ar" })!,
                Text = "Arabic",
                Selected = false
            });
            return model;
        }

        protected virtual InstallModel PrepareCountryList(InstallModel model)
        {
            model.AvailableCountries.Add(new SelectListItem { Value = "", Text = "Select Country" });
            model.AvailableCountries.Add(new SelectListItem { Value = "US-en-US", Text = "United States [English language]" });
            model.AvailableCountries.Add(new SelectListItem { Value = "EG-ar-EG", Text = "Egypt [Arabic language]" });
            return model;
        }
        #endregion

        #region Actions
        [HttpGet]
        public virtual IActionResult Index()
        {
            if (AppDataSettingsManager.IsDatabaseInstalled())
                return RedirectToAction("Index", "Home");

            var model = new InstallModel
            {
                DataProvider = DataProviderType.SqlServer
                // NOTE: Collation remains in the model but not shown in UI (default/null -> provider default)
            };

            PrepareAvailableDataProviders(model);
            PrepareCountryList(model);
            PrepareLanguageList(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Index(InstallModel model)
        {
            if (AppDataSettingsManager.IsDatabaseInstalled())
                return RedirectToAction("Index", "Home");
            if (!ModelState.IsValid)
            {
                PrepareAvailableDataProviders(model);
                PrepareCountryList(model);
                PrepareLanguageList(model);
                return View(model);
            }

            try
            {
                // Step 1: Build the connection string (kept close to old logic)
                var connectionString =
                    model.ConnectionStringRaw
                        ? (model.ConnectionString ?? string.Empty)
                        : BuildSqlServerConnectionString(model);

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    ModelState.AddModelError(string.Empty, "Invalid connection string. Please check your database settings.");
                    PrepareAvailableDataProviders(model);
                    PrepareCountryList(model);
                    PrepareLanguageList(model);
                    return View(model);
                }

                // Step 2: Persist data settings in both managers (as before)
                DataSettingsManager.SaveSettings(new DataConfig
                {
                    DataProvider = model.DataProvider,
                    ConnectionString = connectionString
                }, _fileProvider);

                var dataConfig = new DataConfig
                {
                    ConnectionString = connectionString,
                    DataProvider = DataProviderType.SqlServer
                };
                AppDataSettingsManager.SaveSettings(dataConfig, _fileProvider);

                // Step 3: Create DB if needed (collation kept like old, but not shown in UI → null => provider default)
                if (model.CreateDatabaseIfNotExists && !_dataProvider.DatabaseExists())
                {
                    _dataProvider.CreateDatabase(model.Collation);
                }
                else if (!_dataProvider.DatabaseExists())
                {
                    ModelState.AddModelError(string.Empty, "Database does not exist. Please create the database or enable 'Create database if not exists' option.");
                    PrepareAvailableDataProviders(model);
                    PrepareCountryList(model);
                    PrepareLanguageList(model);
                    return View(model);
                }

                // Step 3.1: Initialize schema
                _dataProvider.InitializeDatabase();

                //await _installationService.InstallSchemaAsync();


                // Step 4: Install required data (current interface is parameterless)
                await _installationService.InstallRequiredDataAsync();
                if (model.LoadSampleData)
                    await _installationService.InstallSampleDataAsync();

                var userRepo = EngineContext.Current.Resolve<IRepository<AppUser>>();
                var roleRepo = EngineContext.Current.Resolve<IRepository<Role>>();
                var userRoleRepo = EngineContext.Current.Resolve<IRepository<UserRole>>();

                var adminRole = (await roleRepo.GetAllAsync(q => q.Where(r => r.SystemName == "Super.Admin.Role"))).FirstOrDefault();
                if (adminRole == null)
                {
                    adminRole = new Role
                    {
                        Name = "Super Admin",
                        SystemName = "Super.Admin.Role",
                        IsActive = true,
						CreatedOnUtc = DateTime.UtcNow
					};
                    await roleRepo.InsertAsync(adminRole);
                }

                var salt = _encryptionService.CreateSaltKey(16);
                var hash = _encryptionService.CreatePasswordHash(model.AdminPassword, salt, PasswordFormat.Hashed);

                var adminUser = new AppUser
                {
                    Username = model.AdminEmail,
                    Email = model.AdminEmail,
                    PasswordSalt = salt,
                    PasswordHash = hash,
                    PasswordFormatId = (int)PasswordFormat.Hashed,
                    IsActive = true,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await userRepo.InsertAsync(adminUser);

                await userRoleRepo.InsertAsync(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                // 4) Sync all permissions
                await _permissionSyncService.SyncPermissionsAsync();

                // 5) Grant all permissions to Admin role
                var allPermissions = await _permissionService.GetAllAsync();
                foreach (var perm in allPermissions)
                {
                    await _permissionService.GrantPermissionToRoleAsync(adminRole.Id, perm.SystemName);
                }

                // (Optional but recommended) make sure System.Admin exists
                var superPerm = await _permissionService.GetBySystemNameAsync(App.Core.Security.AppPermissions.System_Admin);
                if (superPerm != null)
                {
                    await _permissionService.GrantPermissionToRoleAsync(adminRole.Id, superPerm.SystemName);
                }

                var settings = new Setting
                {
                    Name = "sqlConnectionString",
                    Value = connectionString,
                    StoreId = 0,
                };
                _settingRepository.Insert(settings);
                // Step 5: Mark installed & restart
                AppDataSettingsManager.MarkDatabaseAsInstalled();
                _lifetime.StopApplication();

                return Content(
                    "<html><head><meta http-equiv='refresh' content='8;url=/' /></head>" +
                    "<body style='font-family:sans-serif;padding:24px'>" +
                    "<h2>Installation finished</h2>" +
                    "<p>Restarting application... You will be redirected to the Home page automatically.</p>" +
                    "</body></html>", "text/html");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Installation failed: {ex.Message}");
                PrepareAvailableDataProviders(model);
                PrepareCountryList(model);
                PrepareLanguageList(model);
                return View(model);
            }
        }
        #endregion
    }
}
