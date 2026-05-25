using planner_client_package.Interface;
using planner_common_package;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace planner_client_package.Entities
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = Discriminator.TypeDiscriminatorPropertyName)]
    [JsonDerivedType(typeof(InformationBody), Discriminator.Information)]
    [JsonDerivedType(typeof(MeetingBody), Discriminator.Meeting)]
    [JsonDerivedType(typeof(ReminderBody), Discriminator.Reminder)]
    [JsonDerivedType(typeof(TaskBody), Discriminator.Task)]
    public class JobBody : NodeBody
    {
        public string Description { get; set; }

        public int PriorityOrder { get; set; }

        public Status Status { get; set; }

        public JobType JobType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [RegularExpression("^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$")]
        public string HexColor { get; set; }

        public ICollection<AttachedMessageBody> AttachedMessages { get; set; }
        public ReadStateBody? ReadState { get; set; }

        public override IEnumerable<IBody> Extract()
        {
            var result = base.Extract().ToList();

            result.AddIfNotNull(ReadState);
            result.AddRangeIfNotNull(AttachedMessages);

            return result;
        }
    }
}