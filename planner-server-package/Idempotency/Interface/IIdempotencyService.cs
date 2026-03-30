using Microsoft.AspNetCore.Mvc;
using planner_client_package.Entities;
using planner_client_package.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency.Interface
{
    public interface IIdempotencyService
    {
        public Task<ProcessOperation?> GetOperation(Guid opId, Guid accountId, OperationName opName);
        public Task<ServiceResponse<TResult>> ExecuteOperation<TResult>(Guid opId, Guid accountId, OperationName opName, string requestHash, Func<Task<ServiceResponse<TResult>>> handler);
    }
}
