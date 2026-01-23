using planner_server_package.Entities;

namespace planner_node_service.Core.Entities.Models
{
    public class AccessGroupMember
    {
        public Guid Id { get; set; }
        public Guid AccessGroupId { get; set; }
        public AccessGroup AccessGroup { get; set; }
        public Guid AccountId { get; set; }

        public AccessGroupMemberBody ToAccessGroupMemberBody()
        {
            return new AccessGroupMemberBody
            {
                Id = Id,
                AccountId = AccountId,
                GroupId = AccessGroupId
            };
        }
    }
}
