using planner_server_package.Idempotency.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class Response<T>
    {
        public T Body { get; set; }
        public List<ErrorCode> ErrorCodes { get; set; }
    }
}
