using planner_client_package.Enums;
using planner_client_package.Interface;
using System;

namespace planner_client_package.Entities
{
    public class AccessRightBody : ISyncable, IBody
    {
        public Guid Id { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? AccessGroupId { get; set; }
        public AccessGroupBody AccessGroup { get; set; }
        public Guid NodeId { get; set; }
        public NodeBody Node { get; set; }
        public AccessType AccessType { get; set; }
        public NodeType NodeType { get; set; }
    }
}
