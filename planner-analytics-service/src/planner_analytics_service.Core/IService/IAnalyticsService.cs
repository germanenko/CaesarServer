using planner_client_package.Entities;

namespace planner_analytics_service.Core.IService
{
    public interface IAnalyticsService
    {
        public Task<ServiceResponse<AnalyticsActionBody>> AddAction(AnalyticsActionBody action);
        public Task<ServiceResponse<List<AnalyticsActionBody>>> AddActions(List<AnalyticsActionBody> actions);
    }
}
