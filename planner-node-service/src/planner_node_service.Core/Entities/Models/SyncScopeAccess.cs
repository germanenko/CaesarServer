using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Models
{
    public class SyncScopeAccess
    {
        public int Id { get; set; }
        public Guid ScopeId { get; set; }
        public Guid AccountId { get; set; }
        public Permission Permission { get; set; }
        public long RulesRevisionUsed { get; set; }
        public long GraphRevisionUsed { get; set; }
    }
}
