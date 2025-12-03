using Planner_chat_server.Core.Entities.Response;

namespace Planner_chat_server.Core.Entities.Models
{
    public class AccessGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public List<AccessGroupMember> Members { get; set; } = new();

        public AccessGroupBody ToAccessGroupBody()
        {
            return new AccessGroupBody
            {
                Id = Id,
                Name = Name
            };
        }
    }
}
