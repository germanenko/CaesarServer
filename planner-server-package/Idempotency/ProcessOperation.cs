using planner_common_package.Enums;
using planner_server_package.Idempotency.Enum;
using System;

namespace planner_server_package.Idempotency
{
    public class ProcessOperation
    {
        public Guid AccountId { get; set; }
        public OperationName OperationName { get; set; }
        public Guid OperationId { get; set; }
        public string RequestHash { get; set; }
        public ProcessStatus Status { get; set; }
        public DateTime StartAtUtc { get; set; }
        public DateTime CompletedAtUtc { get; set; }
        public string ResultJson { get; set; }
    }
}
