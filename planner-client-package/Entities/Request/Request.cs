using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.Request
{
    public class Request<T>
    {
        public Guid Id { get; set; }
        public T Body { get; set; }
    }
}
