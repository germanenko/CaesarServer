using planner_client_package.Entities;

namespace planner_node_service.Core.Entities.Models
{
    public class AccessGroup : TrackableEntity
    {
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
