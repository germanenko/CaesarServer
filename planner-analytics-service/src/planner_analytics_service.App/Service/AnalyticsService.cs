using FirebaseAdmin.Messaging;
using planner_analytics_service.Core.IRepository;
using planner_analytics_service.Core.IService;
using planner_client_package.Entities;
using System;
using System.Net;

namespace planner_analytics_service.App.Service
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _analyticsRepository;

        public AnalyticsService(
            IAnalyticsRepository analyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
        }

        public async Task<ServiceResponse<AnalyticsActionBody>> AddAction(AnalyticsActionBody action)
        {
            var result = await _analyticsRepository.AddAction(action);

            return new ServiceResponse<AnalyticsActionBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = result
            };
        }

        public async Task<ServiceResponse<List<AnalyticsActionBody>>> AddActions(List<AnalyticsActionBody> actions)
        {
            var result = await _analyticsRepository.AddActions(actions);

            return new ServiceResponse<List<AnalyticsActionBody>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = result
            };
        }
    }
}
