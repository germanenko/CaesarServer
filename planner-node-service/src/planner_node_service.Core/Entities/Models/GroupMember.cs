using planner_client_package.Entities;

namespace planner_node_service.Core.Entities.Models
{
    public class GroupMember
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public GroupAccessSubject Group { get; set; }
        public Guid AccountId { get; set; }

        public AccessGroupMemberBody ToAccessGroupMemberBody()
        {
            return new AccessGroupMemberBody
            {
                Id = Id,
                AccountId = AccountId,
                GroupId = GroupId
            };
        }
    }
}
