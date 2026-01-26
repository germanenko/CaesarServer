using planner_common_package.Enums;

namespace planner_node_service.Core.Entities.Models
{
    public class PublicationStatusModel : ModelBase
    {
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public PublicationStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
