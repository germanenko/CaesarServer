using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Events
{
    public class ScopeUpdatedEvent
    {
        public Guid ScopeId { get; set; }
        public List<Guid> AccountIds { get; set; }
    }
}
