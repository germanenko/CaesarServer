using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Models
{
    public class Node
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public Guid RootId { get; set; }
        public string Name { get; set; }
        public Status Status { get; set; }
        public string? Props { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
    }
}
