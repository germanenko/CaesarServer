using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Models
{
    public class Node
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public Guid ChildId { get; set; }
        public ResourceType ChildType { get; set; }
        public RelationType RelationType { get; set; }
    }
}
