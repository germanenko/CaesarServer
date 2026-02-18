using planner_client_package.Interface;
using System;

namespace planner_client_package.Entities
{
    public class AccessGroupMemberBody : IBody
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid GroupId { get; set; }
    }
}
