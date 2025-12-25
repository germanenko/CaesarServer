using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;

namespace planner_node_service.Core.Entities.Models
{
    public class AccessRight
    {
        public Guid Id { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? AccessGroupId { get; set; }
        public AccessGroup AccessGroup { get; set; }
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public AccessType AccessType { get; set; }
        public NodeType NodeType { get; set; }

        public AccessRightBody ToAccessRightBody()
        {
            return new AccessRightBody
            {
                Id = Id,
                AccountId = AccountId,
                NodeId = NodeId,
                AccessGroupId = AccessGroupId,
                AccessType = AccessType,
                NodeType = NodeType
            };
        }
    }
}
