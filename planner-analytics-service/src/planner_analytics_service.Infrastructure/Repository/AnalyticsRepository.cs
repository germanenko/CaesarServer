using Microsoft.EntityFrameworkCore;
using planner_analytics_service.Core.Entities.Models;
using planner_analytics_service.Core.IRepository;
using planner_analytics_service.Infrastructure.Data;
using planner_client_package.Entities;
using planner_server_package.Converters;

namespace planner_analytics_service.Infrastructure.Repository
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly AnalyticsDbContext _context;

        public AnalyticsRepository(
            AnalyticsDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsActionBody?> AddAction(AnalyticsActionBody actionBody)
        {
            var action = new AnalyticsAction
            {
                Id = actionBody.Id,
                AppVersion = actionBody.AppVersion,
                Connection = actionBody.Connection,
                Description = actionBody.Description,
                Level = actionBody.Level,
                Platform = actionBody.Platform,
                Window = actionBody.Window,
                Date = actionBody.Date
            };

            var result = (await _context.AnalyticsActions.AddAsync(action)).Entity;

            return result.ToBody();
        }

        public async Task<List<AnalyticsActionBody>?> AddActions(List<AnalyticsActionBody> actionBodies)
        {
            var actions = actionBodies
                .Select(x => new AnalyticsAction
                {
                    Id = x.Id,
                    AppVersion = x.AppVersion,
                    Connection = x.Connection,
                    Description = x.Description,
                    Level = x.Level,
                    Platform = x.Platform,
                    Window = x.Window,
                    Date = x.Date
                })
                .ToList();

            await _context.AnalyticsActions.AddRangeAsync(actions);

            await _context.SaveChangesAsync();

            return actions.Select(x => x.ToBody()).ToList();
        }
    }
}
