using planner_client_package.Entities;

namespace planner_node_service.Core.Entities.Models
{
    public class GroupAccessSubject : AccessSubject
    {
        public string Name { get; set; }
        public List<GroupMember> Members { get; set; } = new();

        public GroupAccessSubjectBody ToAccessGroupBody()
        {
            return new GroupAccessSubjectBody
            {
                Id = Id,
                Name = Name
            };
        }

        public override AccessSubjectBody ToBody()
        {
            return new GroupAccessSubjectBody()
            {
                Id = Id,
                Name = Name
            };
        }
    }
}
