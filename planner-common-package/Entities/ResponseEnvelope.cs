using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_common_package.Entities
{
    public class ResponseEnvelope
    {
        public ErrorCode? PrimaryErrorCode { get; set; }
        public List<ErrorCode> ErrorCodes { get; set; }
    }
}
