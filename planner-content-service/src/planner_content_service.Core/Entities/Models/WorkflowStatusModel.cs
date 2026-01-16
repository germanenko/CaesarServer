using planner_server_package.Enums;

namespace planner_content_service.Core.Entities.Models
{
    public class WorkflowStatusModel : ModelBase
    {
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public WorkflowStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
