using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_notify_service.Core.Entities.Models
{
    public class FirebaseToken
    {
        public Guid DeviceId { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}
