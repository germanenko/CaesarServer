using planner_common_package.Enums;
using System;
using System.Net;

namespace planner_server_package.Idempotency
{
    public class OperationFailure
    {
        public Guid OperationId { get; set; }
        public Guid AccountId { get; set; }
        public OperationName OperationName { get; set; }
        public OperationFailureCode OperationFailureCode { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public DateTime FailedAtUtc { get; set; }
        public string Details { get; set; }
    }
}
