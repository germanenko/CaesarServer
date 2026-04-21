using planner_client_package.Interface;
using planner_common_package.Enums;
using System;

namespace planner_client_package.Entities
{
    public class AccessRuleBody : ISyncable, IBody
    {
        public Guid Id { get; set; }
        public AccessSubjectBody AccessSubject { get; set; }
        public Guid NodeId { get; set; }
        public Permission Permission { get; set; }
    }
}
