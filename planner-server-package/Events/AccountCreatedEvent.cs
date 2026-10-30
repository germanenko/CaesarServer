using planner_server_package.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Events
{
    public class AccountCreatedEvent
    {
        public Guid EventId { get; set; }
        public ProfileBody ProfileBody { get; set; }

        public AccountCreatedEvent(ProfileBody profileBody)
        {
            EventId = Guid.NewGuid();
            ProfileBody = profileBody;
        }
    }
}
