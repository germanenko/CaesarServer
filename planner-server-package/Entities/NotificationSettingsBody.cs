using System;

namespace planner_server_package.Entities
{
    public class NotificationSettingsBody
    {
        public Guid NodeId { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
    }
}
