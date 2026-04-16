using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_common_package;
using planner_common_package.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JobBody = planner_client_package.Entities.JobBody;

namespace planner_content_service.Core.Entities.Models
{
    [JsonDerivedType(typeof(Reminder), Discriminator.Reminder)]
    [JsonDerivedType(typeof(Meeting), Discriminator.Meeting)]
    [JsonDerivedType(typeof(Task), Discriminator.Task)]
    [JsonDerivedType(typeof(Information), Discriminator.Information)]
    public abstract class Job : Node
    {
        public Job(
            bool closeWhenChildrenCompleted,
            string? description)
        {
            Description = description;
            CloseWhenChildrenCompleted = closeWhenChildrenCompleted;
        }

        public Guid? PrimarySourceMessageId { get; set; }
        public MessageState? PrimarySourceMessageState { get; set; }
        public string? PrimarySourceMessageSnapshot { get; set; }
        public bool CloseWhenChildrenCompleted { get; set; }
        public string? Description { get; set; }
        public JobType JobType { get; set; }

        [MaxLength(7)]
        public string? HexColor { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public JobBody ToTaskBody()
        {
            return new JobBody
            {
                Id = Id,
                Name = Name,
                Description = Description,
                HexColor = HexColor,
                StartDate = StartDate,
                EndDate = EndDate,
                Props = Props,
                Type = Type,
                JobType = JobType
            };
        }

        public override NodeBody ToNodeBody()
        {
            return new JobBody
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

    public static class JobExtensions
    {
        public static Job WithPeriod(this Job target, DateTime startDate, DateTime endDate)
        {
            target.StartDate = startDate;
            target.EndDate = endDate;

            return target;
        }

        public static Job WithColor(this Job target, string hexColor)
        {
            target.HexColor = hexColor;

            return target;
        }

        public static Job WithPrimarySourceMessage(this Job target, Guid sourceMessageId, string snapshot)
        {
            target.PrimarySourceMessageId = sourceMessageId;
            target.PrimarySourceMessageSnapshot = snapshot;
            target.PrimarySourceMessageState = MessageState.Normal;

            return target;
        }
    }
}