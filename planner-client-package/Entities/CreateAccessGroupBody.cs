using System;
using System.Collections.Generic;

namespace planner_client_package.Entities
{
    public class CreateAccessGroupBody
    {
        public Guid Id;
        public string Name;
        public List<Guid> UserIds;
        public Guid BoardId;
    }
}
