using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace planner_notify_service.Api.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LocalOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var requestHost = context.HttpContext.Request.Host.Host;
            var allowedHosts = new[] { "127.0.0.1", "planner-chat-server", "planner-notify-service" };

            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
            var localIp = context.HttpContext.Connection.LocalIpAddress;

            if (!IPAddress.IsLoopback(remoteIp) && !remoteIp.Equals(localIp))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
