using Microsoft.EntityFrameworkCore;
using planner_node_service.Core.Entities.Models;
using planner_node_service.Core.IRepository;
using planner_node_service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Infrastructure.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NodeDbContext _context;

        public NotificationRepository(NodeDbContext context)
        {
            _context = context;
        }

        public IEnumerable<NotificationSettings> GetEnabledNotificationSettingsAsync(List<Guid> accountIds)
        {
            return _context.NotificationSettings.Where(x => accountIds.Contains(x.AccountId) && x.NotificationsEnabled);
        }
    }
}
