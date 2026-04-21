using planner_client_package.Interface;
using planner_common_package;
using System;
using System.Text.Json.Serialization;

namespace planner_client_package.Entities
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = Discriminator.TypeDiscriminatorPropertyName)]
    [JsonDerivedType(typeof(UserAccessSubjectBody), Discriminator.UserAccessSubject)]
    [JsonDerivedType(typeof(GroupAccessSubjectBody), Discriminator.GroupAccessSubject)]
    public class AccessSubjectBody : IBody, ISyncable
    {
        public Guid Id { get; set; }
    }
}
