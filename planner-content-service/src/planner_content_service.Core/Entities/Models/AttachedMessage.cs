using planner_client_package.Entities;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobId { get; set; }
        public Job Job { get; set; }
        public Guid MessageId { get; set; }
        public string Snapshot { get; set; }
        public MessageState State { get; set; }

        public AttachedMessageBody ToBody()
        {
            return new AttachedMessageBody()
            {
                Id = Id,
                JobId = JobId,
                Snapshot = Snapshot,
                MessageId = MessageId
            };
        }
    }
}
