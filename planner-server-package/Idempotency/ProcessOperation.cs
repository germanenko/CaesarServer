using planner_client_package.Entities.Enum;
using planner_server_package.Idempotency.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency
{
    public class ProcessOperation
    {
        public Guid AccountId { get; set; }
        public OperationName OperationName { get; set; }
        public Guid OperationId { get; set; }
        public string RequestHash { get; set; }
        public Status Status { get; set; }
        public DateTime StartAtUtc { get; set; }
        public DateTime CompletedAtUtc { get; set; }
        public string ResultJson { get; set; }
    }
}
