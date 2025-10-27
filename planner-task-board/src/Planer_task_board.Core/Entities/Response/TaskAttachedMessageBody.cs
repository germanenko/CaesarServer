using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.Entities.Response
{
    public class TaskAttachedMessageBody
    {
        public Guid TaskId { get; set; }
        public Guid MessageId { get; set; }
    }
}
