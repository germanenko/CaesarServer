using planner_client_package.Entities;
using planner_common_package.Enums;

namespace planner_content_service.Core.Entities.Models
{
    public class AttachedMessage
    {
        public AttachedMessage(Guid jobId, Guid messageId, string snapshot)
        {
            JobId = jobId;
            MessageId = messageId;
            Snapshot = snapshot;
            State = MessageState.Normal;
            AttachedAtUtc = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Job Job { get; set; }
        public Guid MessageId { get; set; }
        public string Snapshot { get; set; }
        public MessageState State { get; set; }
        public DateTime AttachedAtUtc { get; set; }

        public AttachedMessageBody ToBody()
        {
            return new AttachedMessageBody()
            {
                Id = Id,
                JobId = JobId,
                Snapshot = Snapshot,
                MessageId = MessageId,
                AttachedAtUtc = AttachedAtUtc
            };
        }
    }
}
