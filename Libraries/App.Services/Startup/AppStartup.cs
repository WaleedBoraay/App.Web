using App.Core;
using App.Core.AppSettingsConfig;
using App.Core.Caching;
using App.Core.Configuration;
using App.Core.Domain.Users;
using App.Core.EngineServices;
using App.Core.Infrastructure;
using App.Core.MigratorServices;
using App.Core.RepositoryServices;
using App.Core.Security;
using App.Core.Singletons;
using App.Core.StartupServices;
using App.Data.Configuration;
using App.Data.DataProviders;
using App.Data.Migrations;
using App.Data.MigratorServices;
using App.Services.Audit;
using App.Services.Authentication;
using App.Services.Common;
using App.Services.Directory;
using App.Services.Files;
using App.Services.Infrastructure.Helper;
using App.Services.Installation;
using App.Services.Institutions;
using App.Services.Localization;
using App.Services.Notifications;
using App.Services.Organization;
using App.Services.Registrations;
using App.Services.Reports;
using App.Services.Security;
using App.Services.Settings;
using App.Services.Templates;
using App.Services.Users;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace App.Services.Startup
{
    public partial class AppStartup : IAppStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)

        {
            var loaded = AppDomain.CurrentDomain.GetAssemblies()
                 .Select(a => a.GetName().Name).OrderBy(n => n);

            var isAppDataLoaded = AppDomain.CurrentDomain.GetAssemblies()
                                      .Any(a => a.GetName().Name.Equals("App.Data", StringComparison.OrdinalIgnoreCase));
            Console.WriteLine("App.Data loaded: " + isAppDataLoaded);

            var corsFile = Path.Combine(AppContext.BaseDirectory, "App_Data", "corssettings.json");
            string[] allowedOrigins = Array.Empty<string>();

            if (File.Exists(corsFile))
            {
                var json = File.ReadAllText(corsFile);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("AllowedOrigins", out var originsElement) && originsElement.ValueKind == JsonValueKind.Array)
                {
                    allowedOrigins = originsElement.EnumerateArray()
                                                   .Select(x => x.GetString()!)
                                                   .Where(x => !string.IsNullOrWhiteSpace(x))
                                                   .ToArray();
                }
            }

            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                });
            });
            services.AddControllers();
            services.AddRazorPages();
            services.AddHttpContextAccessor();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
        });
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            // Register AppSettings
            services.AddSingleton<AppSettings>();

            // Register IAppFileProvider
            services.AddSingleton<IAppFileProvider, AppFileProvider>();

            // Register IStaticCacheManager
            services.AddSingleton<IStaticCacheManager, MemoryCacheManager>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IPermissionSyncService, PermissionSyncService>();
            services.AddScoped<IRoleTemplateSyncService, RoleTemplateSyncService>();


            // Register IInstallationService
            services.AddScoped<IInstallationService, InstallationService>();
            services.AddTransient<InstallRequiredData>();
            services.AddTransient<InstallSampleData>();

            // Register IAppDataProvider
            services.AddScoped<IAppDataProvider, MsSqlNopDataProvider>();

            // Register IRepository<T>
            services.AddScoped(typeof(IRepository<>), typeof(EntityRepository<>));
            // Register IUserService
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserSettingsService, UserSettingsService>();
            services.AddScoped<IInstitutionService, InstitutionService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IStateProvinceService, StateProvinceService>();
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IFileValidationService, FileValidationService>();

            services.AddSingleton<ICacheKeyManager, CacheKeyManager>();
            services.AddSingleton<UserSettings>();
            services.AddTransient(typeof(IConcurrentCollection<>), typeof(ConcurrentTrie<>));
            services.AddSingleton(provider =>
            configuration.GetSection("UserSettings").Get<UserSettings>() ?? new UserSettings());
            // Register IWorkContext
            services.AddScoped<IShortTermCacheManager, PerRequestCacheManager>();
            services.AddScoped<IWorkContext, WorkContext>();
            // Register ILocalizationService
            services.AddScoped<ILocalizationService, LocalizationService>();
            services.AddScoped<ILanguageService, LanguageService>();

            services.AddScoped<IAccessControlService, AccessControlService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRegistrationStatusLogService, RegistrationStatusLogService>();
            services.AddScoped<IRegistrationWorkflowService, RegistrationWorkflowService>();

            //IWorkContext workContext = null;
            services.AddTransient<Lazy<IWorkContext>>();
            services.AddScoped<IGenericAttributeService, GenericAttributeService>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddSingleton<SecuritySettings>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // Register IAccessControlService
            services.AddScoped<IAccessControlService, AccessControlService>();

            // Register INotificationService
            services.AddScoped<INotificationService, NotificationService>();

            // Register IAuditTrailService
            services.AddScoped<IAuditTrailService, AuditTrailService>();
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<IRegistrationSearchService, RegistrationSearchService>();
            services.AddScoped<IBranchService, BranchService>();
            services.AddScoped<IOrganizationsServices, OrganizationsServices>();
            services.AddScoped<IEmailService, EmailService>();

			// Register Organization
			services.AddScoped<ISectorServices, SectorServices>();
			services.AddScoped<IDepartmentServices, DepartmentServices>();
			services.AddScoped<IUnitServices, UnitServices>();

			// Ensure Singleton<IMigrationManager> is initialized after FluentMigrator is configured
			services.AddScoped<IMigrationManager, MigrationManager>();

        }

        public void Configure(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            LocalizationRazorHelper.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());

            var syncService = scope.ServiceProvider.GetRequiredService<IPermissionSyncService>();
            var roleTemplateSync = scope.ServiceProvider.GetRequiredService<IRoleTemplateSyncService>();

            // Fire-and-forget tasks
            Task.Run(async () =>
            {
                try
                {
                    await syncService.SyncPermissionsAsync();
                    await roleTemplateSync.SyncRoleTemplatesAsync();
                }
                catch (Exception ex)
                {
                    // TODO: log exception properly
                    Console.WriteLine($"[ACL Sync Error] {ex.Message}");
                }
            });
            var migrationManager = scope.ServiceProvider.GetRequiredService<IMigrationManager>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .ToList();

            foreach (var assembly in assemblies)
            {
                try
                {
                    migrationManager.ApplyUpUpdateMigrations(assembly);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Migration Error] {ex.Message}");
                }
            }
        }


        public int Order => 100;
    }
}

