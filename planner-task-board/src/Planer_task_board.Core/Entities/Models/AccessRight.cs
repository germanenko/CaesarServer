using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Models
{
    public class AccessRight : Node
    {
        public Guid? AccountId { get; set; }
        public Guid? AccessGroupId { get; set; }
        public AccessGroup AccessGroup { get; set; }
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public AccessType AccessType { get; set; }
        public NodeType ResourceType { get; set; }

        public bool IsGroupAccess => AccessGroupId.HasValue;
        public bool IsIndividualAccess => AccountId.HasValue;
    }
}
