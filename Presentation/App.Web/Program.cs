using App.Core;
using App.Core.Caching;
using App.Core.Configuration;
using App.Core.EngineServices;
using App.Core.Infrastructure;
using App.Core.Singletons;
using App.Core.StartupServices;
using App.Data.EngineServices;
using App.Data.MigratorServices;
using App.Services.Hubs;
using App.Services.Startup;
using App.Web.Infrastructure.Install;
using App.Web.Infrastructure.Mapper.Admin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAutoMapper(typeof(AdminMappingProfile));
builder.Services.AddSignalR();
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddJsonFile(Path.Combine("App_Data", "corssettings.json"), optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
foreach (var kv in builder.Configuration.AsEnumerable())
{
    Console.WriteLine($"{kv.Key} = {kv.Value}");
}
// 0) Ensure controllers/views at minimum (in case startups didn't add them)
builder.Services.AddControllersWithViews();
// 🔹 Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var env = builder.Environment;
CommonHelper.DefaultFileProvider = new AppFileProvider(env);

// Preload assemblies so type finder sees everything
foreach (var file in Directory.EnumerateFiles(AppContext.BaseDirectory, "*.dll"))
{
    try { Assembly.LoadFrom(file); } catch { /* ignore */ }
}

// Init TypeFinder
if (Singleton<ITypeFinder>.Instance == null)
{
    Singleton<ITypeFinder>.Instance = new AppTypeFinder();
}

var typeFinder = Singleton<ITypeFinder>.Instance;

// 1) Discover IAppStartup implementations
var startupInstances = typeFinder.FindClassesOfType<IAppStartup>()
    .Select(t => (IAppStartup)Activator.CreateInstance(t)!)
    .OrderBy(s => s.Order)
    .ToList();

Console.WriteLine("Found IAppStartup classes:");
foreach (var s in startupInstances)
    Console.WriteLine(s.GetType().FullName + " Order: " + s.Order);

// 2) Execute ConfigureServices for all startups
foreach (var startup in startupInstances)
{
    startup.ConfigureServices(builder.Services, builder.Configuration);
}


// 3) Authentication & Authorization (only if not already registered)
if (!builder.Services.Any(sd => sd.ServiceType == typeof(IAuthenticationSchemeProvider)))
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
        });
}
builder.Services.AddAuthorization();

var app = builder.Build();

foreach (var startup in startupInstances)
{
    startup.Configure(app);
}
// 4) Install-guard redirection BEFORE auth/authorization middlewares
app.Use(async (context, next) =>
{
    var path = (context.Request.Path.Value ?? string.Empty).ToLowerInvariant();
    var installed = InstallGuard.IsInstalled();

    var isInstallUrl = path.StartsWith("/install");
    var isStaticFile = path.StartsWith("/lib")
                       || path.StartsWith("/css")
                       || path.StartsWith("/js")
                       || path.StartsWith("/images")
                       || path.StartsWith("/swagger")
                       || path.StartsWith("/favicon");

    if (!installed && !isInstallUrl && !isStaticFile)
    {
        context.Response.Redirect("/Install");
        return;
    }

    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (errorFeature != null)
            {
                var exception = errorFeature.Error;
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(exception, "Unhandled exception in request pipeline");
            }
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An error occurred.");
        });
    });
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");

// 🔹 Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "App API v1");
    c.RoutePrefix = "swagger"; // UI available at /swagger
});
app.MapHub<NotificationHub>("/notificationHub");
app.UseAuthentication();
app.UseAuthorization();

// MVC routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapDefaultControllerRoute();
app.MapRazorPages();

app.Run();
