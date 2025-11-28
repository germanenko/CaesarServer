using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.Entities.Request
{
    public class CreateAccessGroupBody
    {
        public Guid Id;
        public string Name;
        public List<Guid>? UserIds;
        public Guid BoardId;
    }
}
