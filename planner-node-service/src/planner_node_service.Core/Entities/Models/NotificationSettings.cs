using planner_client_package.Entities;

namespace planner_node_service.Core.Entities.Models
{
    public class NotificationSettings
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
    }
}
