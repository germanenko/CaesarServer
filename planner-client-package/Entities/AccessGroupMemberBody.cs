using System;

namespace planner_client_package.Entities
{
    public class AccessGroupMemberBody
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid GroupId { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
