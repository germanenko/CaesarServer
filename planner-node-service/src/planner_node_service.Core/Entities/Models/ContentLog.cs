using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Models
{
    public class ContentLog
    {
        public long Seq { get; set; }
        public Guid ScopeId { get; set; }
        public Guid EntityId { get; set; }
        public long ScopeVersion { get; set; }
        public ActionType Action { get; set; }

        public ContentLog(Guid scopeId, Guid entityId, ActionType action)
        {
            ScopeId = scopeId;
            EntityId = entityId;
            Action = action;
        }
    }
}
