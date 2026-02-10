using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Events
{
    public class CheckAccessRequest
    {
        public Guid AccountId { get; set; }
        public Guid NodeId { get; set; }
    }
}
