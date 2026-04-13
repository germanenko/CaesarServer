using planner_client_package.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public record ScopeVersionBody : IBody
    {
        public Guid ScopeId { get; set; }
        public long Version { get; set; }

        public ScopeVersionBody(Guid scopeId, long version)
        {
            ScopeId = scopeId;
            Version = version;
        }
    }
}
