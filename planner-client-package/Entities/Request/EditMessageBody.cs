using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.Request
{
    public class EditMessageBody
    {
        public Guid MessageId { get; set; }
        public string Content { get; set; }
    }
}
