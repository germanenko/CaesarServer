using planner_client_package.Entities.Enum;
using planner_common_package.Enums;
using planner_server_package.Idempotency.Enum;
using planner_server_package.Idempotency.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

        public async Task<ServiceResponse<TResult>> ExecuteOperation<TResult>(Guid opId, Guid accountId, OperationName opName, object request, Func<Task<ServiceResponse<TResult>>> handler)
        {
            var operation = await GetOperation(opId, accountId, opName);

            var requestHash = ComputeRequestHash(request);

            if (operation != null)
            {
                if (operation.RequestHash == requestHash)
                {
                    if (operation.Status == ProcessStatus.Complete)
                    {
                        return JsonSerializer.Deserialize<ServiceResponse<TResult>>(operation.ResultJson);
                    }
                    else
                    {
                        return new ServiceResponse<TResult>()
                        {
                            StatusCode = System.Net.HttpStatusCode.Accepted,
                            IsSuccess = true
                        };
                    }
                }
            }
            else
            {
                await _idempotencyRepository.AddOperation(opId, accountId, opName, requestHash);

                var result = await handler();

                if (result.IsSuccess == false)
                {
                    var errorKind = result.ErrorCodes.Max().GetErrorKind();

                    await _idempotencyRepository.SetOperationFailed(opId, result.StatusCode, errorKind, result.Errors);
                }
                else
                {
                    await _idempotencyRepository.SetOperationCompleted(opId, JsonSerializer.Serialize(result));
                }

                return result;
            }

            return new ServiceResponse<TResult>()
            {
                StatusCode = System.Net.HttpStatusCode.Conflict,
                IsSuccess = false,
                ErrorCodes = [ErrorCode.DuplicateRequest],
                Errors = ["Запрос с таким Id уже существует"]
            };
        }

        public string ComputeRequestHash(object requestBody)
        {
            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
            });

            var bytes = Encoding.UTF8.GetBytes(json);
            var hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
