using CaesarServerLibrary.Enums;

namespace planner_chat_service.Core.Entities.Models
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

        public bool IsGroupAccess => AccessGroupId.HasValue;
        public bool IsIndividualAccess => AccountId.HasValue;
    }
}
