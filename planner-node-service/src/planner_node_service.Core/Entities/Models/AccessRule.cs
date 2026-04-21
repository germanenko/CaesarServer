using planner_client_package.Entities;
using planner_common_package.Enums;

namespace planner_node_service.Core.Entities.Models
{
    public class AccessRule
    {
        public Guid Id { get; set; }
        public Guid SubjectId { get; set; }
        public AccessSubject Subject { get; set; }
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public Permission Permission { get; set; }

        public AccessRuleBody ToBody()
        {
            return new AccessRuleBody
            {
                Id = Id,
                AccessSubject = Subject.ToBody(),
                NodeId = NodeId,
                Permission = Permission
            };
        }
    }
}
