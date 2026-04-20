using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace planner_node_service.Api.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LocalOnlyAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var requestHost = context.HttpContext.Request.Host.Host;
            var allowedHosts = new[] { "127.0.0.1", "planner-content-service", "planner-chat-service", "planner-notify-service" };

            if (!allowedHosts.Contains(requestHost))
            {
                Console.WriteLine(requestHost);
                context.Result = new ForbidResult();
            }
        }
    }
}
