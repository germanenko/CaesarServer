using planner_server_package.Enums;

namespace planner_content_service.Core.Entities.Models
{
    public class PublicationStatusModel : ModelBase
    {
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public PublicationStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
