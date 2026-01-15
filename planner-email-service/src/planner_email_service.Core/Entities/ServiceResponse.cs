using System.Net;

namespace planner_email_service.Core.Entities
{
    public class ServiceResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public T? Body { get; set; }
        public string[] Errors { get; set; }
    }
}