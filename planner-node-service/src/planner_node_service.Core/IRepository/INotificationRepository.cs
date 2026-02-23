using planner_node_service.Core.Entities.Models;
using planner_server_package.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.IRepository
{
    public interface INotificationRepository
    {
        public Task<List<NotificationSettings>> GetEnabledNotificationSettingsAsync(List<Guid> accountIds);
        public Task<NotificationSettings> AddNotificationSettings(NotificationSettingsBody notificationSettingsBody);
    }
}
