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
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;
            var requestHost = context.HttpContext.Request.Host.Host;

            if (remoteIp != null && (
                IPAddress.IsLoopback(remoteIp) ||
                remoteIp.ToString().StartsWith("172.")))
            {
                return;
            }

            var allowedHosts = new[]
            {
                "127.0.0.1",
                "localhost",
                "planner-chat-service",
                "planner_notify_service"
            };

            if (allowedHosts.Contains(requestHost))
            {
                return;
            }

            context.Result = new ForbidResult();
        }
    }
}
