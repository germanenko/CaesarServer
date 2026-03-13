using planner_client_package.Entities;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Core.IService;
using System.Net;

namespace planner_node_service.App.Service
{
    public class LogService : ILogService
    {
        private readonly ILogRepository _logRepository;

        public LogService(ILogRepository historyRepository)
        {
            _logRepository = historyRepository;
        }

        public async Task<ServiceResponse<History>> AddHistory(History history)
        {
            var result = await _logRepository.AddHistory(history);

            if (result == null)
            {
                return new ServiceResponse<History>()
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Ошибка добавления истории" }
                };
            }

            return new ServiceResponse<History>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }

        public async Task<ServiceResponse<ContentLog>> AddContentLog(ContentLog log)
        {
            var result = await _logRepository.AddContentLog(log);

            if (result == null)
            {
                return new ServiceResponse<ContentLog>()
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "Ошибка добавления лога" }
                };
            }

            return new ServiceResponse<ContentLog>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result
            };
        }

        public async Task<ServiceResponse<History>> GetCreateHistory(Guid nodeId)
        {
            var history = await _logRepository.GetCreateHistory(nodeId);

            if (history == null)
            {
                return new ServiceResponse<History>()
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new[] { "История не найдена" }
                };
            }

            return new ServiceResponse<History>()
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = history
            };
        }
    }
}
