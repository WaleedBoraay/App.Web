using App.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace App.Web.Infrastructure.Security
{
    /// <summary>
    /// Custom attribute for permission-based authorization
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class PermissionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permissionSystemName;

        public PermissionAuthorizeAttribute(string permissionSystemName)
        {
            _permissionSystemName = permissionSystemName;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (string.IsNullOrEmpty(_permissionSystemName))
                return;

            var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();

            // جِب الـ UserId من الـ Claims
            var userIdClaim = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var userId = int.Parse(userIdClaim.Value);

            var authorized = await permissionService.AuthorizeAsync(userId, _permissionSystemName);
            if (!authorized)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
