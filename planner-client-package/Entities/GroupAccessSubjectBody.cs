using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class GroupAccessSubjectBody : AccessSubjectBody
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
