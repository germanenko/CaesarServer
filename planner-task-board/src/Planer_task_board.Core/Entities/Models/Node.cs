using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Models
{
    public class Node : ModelBase
    {
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string? Props { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }

        public NodeBody ToNodeBody()
        {
            return new NodeBody()
            {
                Id = Id,
                Name = Name,
                Props = Props,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                CreatedBy = CreatedBy,
                UpdatedBy = UpdatedBy,
                Type = Type
            };
        }
    }
}
