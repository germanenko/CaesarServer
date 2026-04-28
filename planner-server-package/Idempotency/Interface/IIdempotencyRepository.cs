using planner_client_package.Entities.Enum;
using planner_common_package.Enums;
using planner_server_package.Idempotency.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency.Interface
{
    public interface IIdempotencyRepository
    {
        public Task<ProcessOperation?> GetOperation(Guid opId, Guid accountId, OperationName opName, CancellationToken cancellationToken);
        public Task<ProcessOperation?> AddOperation(Guid opId, Guid accountId, OperationName opName, string requestHash, CancellationToken cancellationToken);
        public Task<ProcessOperation?> SetOperationCompleted(Guid opId, string responseBody, CancellationToken cancellationToken);
        public Task<ProcessOperation?> SetOperationFailed(Guid opId, HttpStatusCode httpStatusCode, OperationFailureCode operationFailureCode, string[] errors, CancellationToken cancellationToken);
        public Task DeleteStuckRequests(CancellationToken cancellationToken);
        public Task DeleteOldRequests(CancellationToken cancellationToken);
    }
}
