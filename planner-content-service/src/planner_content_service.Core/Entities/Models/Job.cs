using planner_client_package.Entities;
using planner_common_package;
using planner_common_package.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace planner_content_service.Core.Entities.Models
{
    [JsonDerivedType(typeof(Reminder), Discriminator.Reminder)]
    [JsonDerivedType(typeof(Meeting), Discriminator.Meeting)]
    [JsonDerivedType(typeof(Task), Discriminator.Task)]
    [JsonDerivedType(typeof(Information), Discriminator.Information)]
    public class Job : Node
    {
        public Guid PrimarySourceMessageId { get; set; }
        public MessageSnapshot PrimarySourceSnapshot { get; set; }
        public MessageState PrimarySourceMessageState { get; set; }
        public bool CloseWhenChildrenCompleted { get; set; }
        public string? Description { get; set; }

        [MaxLength(7)]
        public string? HexColor { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public TaskBody ToTaskBody()
        {
            return new TaskBody
            {
                Id = Id,
                Name = Name,
                Description = Description,
                HexColor = HexColor,
                StartDate = StartDate,
                EndDate = EndDate,
                Props = Props,
                Type = Type
            };
        }

        public override NodeBody ToNodeBody()
        {
            return new TaskBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type,
                Description = Description,
                HexColor = HexColor,
                StartDate = StartDate,
                EndDate = EndDate,
            };
        }
    }
}