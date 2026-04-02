using planner_client_package.Entities.Enum;
using planner_server_package.Idempotency.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency
{
    public class OperationFailure
    {
        public Guid OperationId { get; set; }
        public Guid AccountId { get; set; }
        public OperationName OperationName { get; set; }
        public ErrorCode ErrorCode { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public DateTime FailedAtUtc { get; set; }
        public string Details { get; set; }
    }
}
