using planner_client_package.Entities;
using planner_client_package.Entities.Enum;
using planner_server_package.Idempotency.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency
{
    public class IdempotencyService : IIdempotencyService
    {
        public readonly IIdempotencyRepository _idempotencyRepository;

        public IdempotencyService(IIdempotencyRepository idempotencyRepository)
        {
            _idempotencyRepository = idempotencyRepository;
        }

        public async Task<ProcessOperation> GetOperation(Guid opId, Guid accountId, OperationName opName)
        {
            var operation = await _idempotencyRepository.GetOperation(opId, accountId, opName);

            return operation;
        }

        public async Task<ServiceResponse<TResult>> ExecuteOperation<TResult>(Guid opId, Guid accountId, OperationName opName, string requestHash, Func<Task<ServiceResponse<TResult>>> handler)
        {
            var operation = await GetOperation(opId, accountId, opName);

            if (operation != null)
            {
                if (operation.RequestHash == requestHash && operation.Status == Status.Complete)
                {
                    return JsonSerializer.Deserialize<ServiceResponse<TResult>>(operation.ResultJson);
                }
            }
            else
            {
                await _idempotencyRepository.AddOperation(opId, accountId, opName, requestHash);

                var result = await handler();

                if (result.IsSuccess == false)
                {
                    await _idempotencyRepository.SetOperationFailed(opId, result.StatusCode, result.Errors);
                }
                else
                {
                    await _idempotencyRepository.SetOperationCompleted(opId, JsonSerializer.Serialize(result));
                }

                return result;
            }

            return null;
        }
    }
}
