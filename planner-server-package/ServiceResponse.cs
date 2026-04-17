using planner_common_package.Enums;
using System.Collections.Generic;
using System.Net;

namespace planner_server_package
{
    public class ServiceResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public T? Body { get; set; }
        public List<ErrorCode> ErrorCodes { get; set; }
        public string[] Errors { get; set; }
    }
}
