using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class CreateChatBody
    {
        public Guid Id { get; set; }
        public Guid CompanionId { get; set; }
    }
}
