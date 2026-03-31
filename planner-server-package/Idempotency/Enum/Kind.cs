using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency.Enum
{
    public enum Kind
    {
        AccessDenied,
        Validation,
        Conflict,
        Infrastructure
    }
}
