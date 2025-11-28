using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.Entities.Models
{
    public class AccessGroup : ModelBase
    {
        public string Name { get; set; } 
        public List<AccessGroupMember> Members { get; set; } = new();
    }
}
