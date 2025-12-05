using Planer_task_board.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.Entities.Request
{
    public class CreateOrUpdateNodeLink
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public Guid ChildId { get; set; }
        public RelationType RelationType { get; set; }
    }
}
