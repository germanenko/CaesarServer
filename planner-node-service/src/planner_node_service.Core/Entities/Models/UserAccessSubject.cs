using planner_client_package.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace planner_node_service.Core.Entities.Models
{
    public class UserAccessSubject : AccessSubject
    {
        public Guid AccountId { get; set; }

        public override AccessSubjectBody ToBody()
        {
            return new UserAccessSubjectBody()
            {
                Id = Id,
                AccountId = AccountId
            };
        }
    }
}
