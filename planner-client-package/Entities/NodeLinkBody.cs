using planner_client_package.Interface;
using planner_common_package.Enums;
using System;

namespace planner_client_package.Entities
{
    public class NodeLinkBody : IBody, ISyncable
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public Guid ChildId { get; set; }
        public RelationType RelationType { get; set; }
    }
}
