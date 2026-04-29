using planner_client_package.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.Request
{
    public class TaskColumnRequest : IRequest
    {
        public Guid Id { get; set; }
        public Guid? ChatId { get; set; }
        public Guid ColumnId { get; set; }
    }
}
