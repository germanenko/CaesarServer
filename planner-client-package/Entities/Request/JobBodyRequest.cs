using planner_client_package.Interface;
using planner_common_package;
using planner_common_package.Enums;
using System;
using System.Text.Json.Serialization;

namespace planner_client_package.Entities.Request
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = Discriminator.TypeDiscriminatorPropertyName)]
    [JsonDerivedType(typeof(MeetingBodyRequest), Discriminator.Meeting)]
    [JsonDerivedType(typeof(ReminderBodyRequest), Discriminator.Reminder)]
    [JsonDerivedType(typeof(InformationBodyRequest), Discriminator.Information)]
    [JsonDerivedType(typeof(TaskBodyRequest), Discriminator.Task)]
    public abstract class JobBodyRequest : IRequest
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Props { get; set; }

        public string Description { get; set; }

        public JobType Type { get; set; }

        public NodeLinkBody Link { get; set; }
    }
}
