using System.Net;

namespace Planner_Auth.Core.Entities.Response
{
    public class ServiceResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public T? Body { get; set; }
        public string[] Errors { get; set; }
    }
}