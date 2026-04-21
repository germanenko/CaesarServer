using planner_client_package.Interface;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;

namespace planner_client_package.Entities
{
    public class AccessRuleBody : ISyncable, IBody
    {
        public Guid Id { get; set; }
        public AccessSubjectBody AccessSubject { get; set; }
        public Guid NodeId { get; set; }
        public Permission Permission { get; set; }
    }

    public static class AccessRuleExtensions
    {
        public static IEnumerable<IBody> Extract<T>(this T target) where T : AccessRuleBody
        {
            List<IBody> result = new List<IBody> { target };

            var subject = target.AccessSubject;

            if (subject != null)
            {
                result.AddIfNotNull(subject);

                if (subject is UserAccessSubjectBody user)
                {
                    result.AddIfNotNull(user.Profile);
                }
                else if (subject is GroupAccessSubjectBody group)
                {
                    result.AddRangeIfNotNull(group.Members);
                }
            }

            return result;
        }
    }
}
