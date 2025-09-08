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
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
            var isLocal = remoteIp.Equals(IPAddress.Loopback) ||
                         remoteIp.Equals(IPAddress.IPv6Loopback) ||
                         IPAddress.IsLoopback(remoteIp);

            if (!isLocal)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
