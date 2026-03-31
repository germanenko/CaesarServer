using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using planner_client_package.Entities.Enum;
using planner_server_package.Idempotency.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly IdempotencyContext _context;

        public IdempotencyRepository(IdempotencyContext context)
        {
            _context = context;
        }

        public async Task<ProcessOperation> GetOperation(Guid opId, Guid accountId, OperationName opName)
        {
            return await _context.ProcessedOperations.AsNoTracking().FirstOrDefaultAsync(x => x.OperationId == opId && x.AccountId == accountId && x.OperationName == opName);
        }

        public async Task<ProcessOperation> AddOperation(Guid opId, Guid accountId, OperationName opName, string requestHash)
        {
            var operation = await _context.ProcessedOperations.AddAsync(new ProcessOperation()
            {
                OperationId = opId,
                AccountId = accountId,
                OperationName = opName,
                RequestHash = requestHash,
                StartAtUtc = DateTime.UtcNow,
                Status = Status.InProgress
            });

            await _context.SaveChangesAsync();

            return await _context.ProcessedOperations.FirstOrDefaultAsync(x => x.OperationId == opId && x.AccountId == accountId && x.OperationName == opName);
        }

        public async Task<ProcessOperation> SetOperationCompleted(Guid opId, string responseBody)
        {
            var operation = await _context.ProcessedOperations.FirstOrDefaultAsync(x => x.OperationId == opId);

            operation.Status = Status.Complete;
            operation.ResultJson = responseBody;
            operation.CompletedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return operation;
        }

        public async Task<ProcessOperation> SetOperationFailed(Guid opId, HttpStatusCode httpStatusCode, string[] errors)
        {
            var operation = await _context.ProcessedOperations.FirstOrDefaultAsync(x => x.OperationId == opId);

            _context.ProcessedOperations.Remove(operation);

            var kind = Kind.AccessDenied;

            switch (httpStatusCode)
            {
                case HttpStatusCode.Forbidden:
                    kind = Kind.AccessDenied;
                    break;
                case HttpStatusCode.BadRequest:
                    kind = Kind.Validation;
                    break;
                case HttpStatusCode.InternalServerError:
                    kind = Kind.Infrastructure;
                    break;
                case HttpStatusCode.Conflict:
                    kind = Kind.Conflict;
                    break;
            }


            await _context.OperationFailures.AddAsync(new OperationFailure()
            {
                AccountId = operation.AccountId,
                FailedAtUtc = DateTime.UtcNow,
                HttpStatusCode = httpStatusCode,
                Kind = kind,
                OperationId = operation.OperationId,
                OperationName = operation.OperationName,
                Details = string.Join(", ", errors)
            });

            operation.Status = Status.Complete;

            await _context.SaveChangesAsync();

            return operation;
        }
    }
}
