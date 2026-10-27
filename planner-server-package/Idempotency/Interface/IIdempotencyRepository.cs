using planner_common_package.Enums;
using System;
using System.Net;
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
