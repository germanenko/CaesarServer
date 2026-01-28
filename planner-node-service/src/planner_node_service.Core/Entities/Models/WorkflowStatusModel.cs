using planner_common_package.Enums;

namespace planner_node_service.Core.Entities.Models
{
    public class WorkflowStatusModel : StatusBase
    {
        public WorkflowStatus Status { get; set; }
    }
}
