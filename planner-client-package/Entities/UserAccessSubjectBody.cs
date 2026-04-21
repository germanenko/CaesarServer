using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class UserAccessSubjectBody : AccessSubjectBody
    {
        public Guid AccountId { get; set; }
        public ProfileBody Profile { get; set; }
    }
}
