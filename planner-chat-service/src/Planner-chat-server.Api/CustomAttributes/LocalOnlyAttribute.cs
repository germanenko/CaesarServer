using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Planner_chat_server.Api.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LocalOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var requestHost = context.HttpContext.Request.Host.Host;
            var allowedHosts = new[] { "127.0.0.1", "planner-chat-server" };

            if (!allowedHosts.Contains(requestHost))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
