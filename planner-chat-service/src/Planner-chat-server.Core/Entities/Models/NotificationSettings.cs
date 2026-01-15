namespace Planner_chat_server.Core.Entities.Models
{
    public class NotificationSettings : ModelBase
    {
        public Guid AccountId { get; set; }
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
    }
}
