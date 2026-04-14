using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Core.Entities.Models
{
    public class UserTaskColumn
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid ColumnId { get; set; }
        public Guid? ChatId { get; set; }
    }
}
