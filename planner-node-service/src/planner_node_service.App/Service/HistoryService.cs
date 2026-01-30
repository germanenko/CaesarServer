using planner_client_package.Entities;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Core.IService;
using System.Net;

namespace planner_node_service.App.Service
{
    public class HistoryService : IHistoryService
    {
        private readonly IHistoryRepository _historyRepository;

        public HistoryService(IHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public async Task<ServiceResponse<History>> AddHistory(History history)
        {
            var result = await _historyRepository.AddHistory(history);

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

        public async Task<ServiceResponse<History>> GetCreateHistory(Guid nodeId)
        {
            var history = await _historyRepository.GetCreateHistory(nodeId);

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
