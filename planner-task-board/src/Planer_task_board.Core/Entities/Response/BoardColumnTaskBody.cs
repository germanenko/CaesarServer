using Planer_task_board.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.Entities.Response
{
    public class BoardColumnTaskBody
    {
        public Guid ColumnId { get; set; }

        public Guid TaskId { get; set; }
    }
}
