using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace planner_notify_service.Core.Entities.Response
{
    public class ServiceResponse<T>
    {
        public HttpStatusCode StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public T? Body { get; set; }
        public string[] Errors { get; set; }
    }
}
