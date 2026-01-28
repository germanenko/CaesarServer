using planner_common_package.Enums;
using planner_server_package.Entities;

namespace planner_node_service.Core.Entities.Models
{
    public class AccessRight : TrackableEntity
    {
        public Guid? AccountId { get; set; }
        public Guid? AccessGroupId { get; set; }
        public AccessGroup AccessGroup { get; set; }
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public AccessType AccessType { get; set; }

        public AccessRightBody ToAccessRightBody()
        {
            return new AccessRightBody
            {
                Id = Id,
                AccountId = AccountId,
                NodeId = NodeId,
                AccessGroupId = AccessGroupId,
                AccessType = AccessType
            };
        }
    }
}
