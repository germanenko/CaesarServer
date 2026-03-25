using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_chat_service.Core.Entities.Models
{
    public class UserHiddenMessage
    {
        public Guid MessageId { get; set; }
        public Guid UserId { get; set; }
    }
}
