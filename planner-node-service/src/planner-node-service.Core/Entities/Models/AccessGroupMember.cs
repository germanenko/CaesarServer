using CaesarServerLibrary.Entities;
using planner_node_service.Core.Entities.Response;

namespace planner_node_service.Core.Entities.Models
{
    public class AccessGroupMember
    {
        public Guid Id { get; set; }
        public Guid AccessGroupId { get; set; }
        public AccessGroup AccessGroup { get; set; }
        public Guid AccountId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public AccessGroupMemberBody ToAccessGroupMemberBody()
        {
            return new AccessGroupMemberBody
            {
                Id = Id,
                AccountId = AccountId,
                GroupId = AccessGroupId,
                JoinedAt = JoinedAt
            };
        }
    }
}
