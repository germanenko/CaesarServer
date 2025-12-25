using System.Collections.Generic;

namespace CaesarServerLibrary.Entities
{
    public class AccessBody
    {
        public List<AccessRightBody> AccessRights { get; set; }
        public List<AccessGroupBody> AccessGroups { get; set; }
        public List<AccessGroupMemberBody> AccessGroupMembers { get; set; }
        public List<ProfileBody> Profiles { get; set; }
    }
}
