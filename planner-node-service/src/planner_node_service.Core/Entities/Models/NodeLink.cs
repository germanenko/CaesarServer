using planner_client_package.Entities;
using planner_common_package.Enums;

namespace planner_node_service.Core.Entities.Models
{
    public class NodeLink : TrackableEntity
    {
        public Guid ParentId { get; set; }
        public Node ParentNode { get; set; }
        public Guid ChildId { get; set; }
        public Node ChildNode { get; set; }
        public RelationType RelationType { get; set; }

        public NodeLinkBody ToBody()
        {
            return new NodeLinkBody()
            {
                Id = Id,
                ParentId = ParentId,
                ChildId = ChildId,
                RelationType = RelationType
            };
        }
    }
}
