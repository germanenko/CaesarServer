using planner_client_package.Entities.Enum;
using planner_common_package.Enums;
using planner_server_package.Idempotency.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency.Interface
{
    public interface IIdempotencyRepository
    {
        public Task<ProcessOperation?> GetOperation(Guid opId, Guid accountId, OperationName opName);
        public Task<ProcessOperation?> AddOperation(Guid opId, Guid accountId, OperationName opName, string requestHash);
        public Task<ProcessOperation?> SetOperationCompleted(Guid opId, string responseBody);
        public Task<ProcessOperation?> SetOperationFailed(Guid opId, HttpStatusCode httpStatusCode, OperationFailureCode operationFailureCode, string[] errors);
        public Task DeleteStuckRequests();
        public Task DeleteOldRequests();
    }
}
