using planner_client_package.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Models
{
    public abstract class AccessSubject
    {
        public Guid Id { get; set; }

        public abstract AccessSubjectBody ToBody();
    }
}
