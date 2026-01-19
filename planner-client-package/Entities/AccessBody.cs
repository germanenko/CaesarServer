using planner_client_package.Interface;
using System.Collections.Generic;

namespace planner_client_package.Entities
{
    public class AccessBody : ISyncable
    {
        public List<AccessRightBody> AccessRights { get; set; }
        public List<AccessGroupBody> AccessGroups { get; set; }
        public List<AccessGroupMemberBody> AccessGroupMembers { get; set; }
        public List<ProfileBody> Profiles { get; set; }
    }
}
