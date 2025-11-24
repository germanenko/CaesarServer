using Planer_task_board.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.Entities.Models
{
    public class AccessRight
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid ResourceId { get; set; }
        public AccessType AccessType { get; set; }
        public NodeType ResourceType { get; set; }
    }
}
