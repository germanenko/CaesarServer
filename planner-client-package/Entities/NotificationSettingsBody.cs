using planner_client_package.Interface;
using System;

namespace planner_client_package.Entities
{
    public class NotificationSettingsBody : ISyncable
    {
        public Guid NodeId { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
    }
}
