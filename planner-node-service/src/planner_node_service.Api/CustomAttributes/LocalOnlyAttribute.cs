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
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;

            if (remoteIp == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var ip = remoteIp.MapToIPv4().ToString();

            if (ip == "127.0.0.1" || ip == "::1")
                return;

            if (ip.StartsWith("172.18."))
                return;

            Console.WriteLine(ip);
            context.Result = new ForbidResult();
        }
    }
}
