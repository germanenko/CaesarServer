using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Models
{
    public class NodeLink : ModelBase
    {
        public Guid ParentId { get; set; }
        public Node ParentNode { get; set; }
        public NodeType ParentType { get; set; }
        public Guid ChildId { get; set; }
        public Node ChildNode { get; set; }
        public NodeType ChildType { get; set; }
        public RelationType RelationType { get; set; }
        public Guid RootId { get; set; }
    }
}
