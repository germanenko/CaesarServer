using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Core.Entities.Models
{
    public class Task : Job
    {
        public List<Guid> PermormerIds { get; set; }
    }
}
