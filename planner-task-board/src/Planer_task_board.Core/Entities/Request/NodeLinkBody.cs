using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Request
{
    public class NodeLinkBody
    {
        public Guid ParentId { get; set; }
        public Guid ChildId { get; set; }
        public RelationType RelationType { get; set; }
    }
}
