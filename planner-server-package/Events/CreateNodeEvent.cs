using planner_server_package.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Events
{
    public class CreateNodeEvent
    {
        public NodeBody Node { get; set; }
        public Guid CreatorId { get; set; }
    }
}
