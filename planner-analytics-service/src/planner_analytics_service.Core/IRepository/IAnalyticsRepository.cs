using planner_analytics_service.Core.Entities.Models;
using planner_client_package.Entities;
using System;

namespace planner_analytics_service.Core.IRepository
{
    public interface IAnalyticsRepository
    {
        Task<AnalyticsActionBody?> AddAction(AnalyticsActionBody actionBody);
        Task<List<AnalyticsActionBody>?> AddActions(List<AnalyticsActionBody> actionBodies);
    }
}
