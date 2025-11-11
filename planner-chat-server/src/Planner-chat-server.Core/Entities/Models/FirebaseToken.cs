using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner_chat_server.Core.Entities.Models
{
    public class FirebaseToken
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}
