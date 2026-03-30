using planner_client_package.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_chat_service.Core.Entities.ValueObjects
{
    public record MessagePreview
    {
        public Guid MessageId { get; set; }
        public Guid AuthorId { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; }

        public MessagePreviewBody ToBody()
        {
            return new MessagePreviewBody()
            {
                AuthorId = AuthorId,
                Text = Text,
                SentAt = SentAt,
                MessageId = MessageId
            };
        }
    }
}
