using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_common_package.Enums
{
    public enum OperationFailureCode
    {
        Validation = 100,
        Conflict = 200,
        Infrastructure = 300,
        AccessDenied = 400,
    }
}
