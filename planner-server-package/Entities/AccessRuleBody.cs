using planner_client_package.Entities;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace planner_server_package.Entities
{
    public class AccessRuleBody
    {
        public Guid Id { get; set; }
        public AccessSubjectBody AccessSubject { get; set; }
        public Guid NodeId { get; set; }
        public Permission Permission { get; set; }
    }
}
