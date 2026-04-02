using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Idempotency.Enum
{
    public enum ErrorCode
    {
        Validation = 100,
        Conflict = 200,
        Infrastructure = 300,
        AccessDenied = 400,
    }
}
