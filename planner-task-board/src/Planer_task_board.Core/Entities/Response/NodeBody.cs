using Planer_task_board.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.Entities.Response
{
    public class NodeBody
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string? Props { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
    }
}
