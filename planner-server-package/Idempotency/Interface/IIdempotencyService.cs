using planner_common_package.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency.Interface
{
    public interface IIdempotencyService
    {
        public Task<ProcessOperation?> GetOperation(Guid opId, Guid accountId, OperationName opName, CancellationToken cancellationToken);
        public Task<ServiceResponse<TResult>> ExecuteOperation<TResult>(Guid opId, Guid accountId, OperationName opName, object request, Func<Task<ServiceResponse<TResult>>> handler, CancellationToken cancellationToken);
    }
}
