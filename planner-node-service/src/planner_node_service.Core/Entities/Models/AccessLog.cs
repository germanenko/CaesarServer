using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Models
{
    public class AccessLog
    {
        public int Seq { get; set; }
        public Guid SubjectId { get; set; }
        public Node Scope { get; set; }
        public Guid ScopeId { get; set; }
        public Permission Permission { get; set; }
        public long RulesRevision { get; set; }
        public long GraphRevision { get; set; }
    }
}
