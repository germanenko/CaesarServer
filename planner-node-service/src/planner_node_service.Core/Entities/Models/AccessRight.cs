using planner_client_package.Entities;
using planner_common_package.Enums;

namespace planner_node_service.Core.Entities.Models
{
    public class AccessRight : TrackableEntity
    {
        public Guid? AccountId { get; set; }
        public Guid? AccessGroupId { get; set; }
        public AccessGroup AccessGroup { get; set; }
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public Permission Permission { get; set; }

        public AccessRightBody ToAccessRightBody()
        {
            return new AccessRightBody
            {
                Id = Id,
                AccountId = AccountId,
                NodeId = NodeId,
                AccessGroupId = AccessGroupId,
                Permission = Permission
            };
        }
    }
}
