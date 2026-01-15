using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Planer_email_service.Api.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LocalOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var requestHost = context.HttpContext.Request.Host.Host;
            var allowedHosts = new[] { "planer-email-service" };

            if (!allowedHosts.Contains(requestHost))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
