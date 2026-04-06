using planner_client_package.Interface;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace planner_client_package.Entities
{
    public class AccessBody : ISyncable, IBody
    {
        [JsonConstructor]
        public AccessBody() { }

        public List<AccessRuleBody> AccessRules { get; set; } = new List<AccessRuleBody>();
        public List<AccessGroupBody> AccessGroups { get; set; } = new List<AccessGroupBody>();
        public List<AccessGroupMemberBody> AccessGroupMembers { get; set; } = new List<AccessGroupMemberBody>();
        public List<ProfileBody> Profiles { get; set; } = new List<ProfileBody>();

    }
}
