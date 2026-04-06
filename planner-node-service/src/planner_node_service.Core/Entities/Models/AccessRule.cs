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

        public AccessRightBody ToBody()
        {
            return new AccessRightBody
            {
                Id = Id,
                AccountId = SubjectId,
                NodeId = NodeId,
                Permission = Permission
            };
        }
    }
}
