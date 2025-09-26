using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace App.Services.Installation
{
    public class InstallUrlMiddleware
    {
        private readonly RequestDelegate _next;

        public InstallUrlMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // redirect to install URL if needed
            await _next(context);
        }
    }
}
