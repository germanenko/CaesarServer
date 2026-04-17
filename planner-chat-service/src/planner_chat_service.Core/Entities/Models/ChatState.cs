using planner_chat_service.Core.Entities.ValueObjects;
using planner_client_package.Entities;

namespace planner_chat_service.Core.Entities.Models
{
    public class ChatState
    {
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
        public long? LastMessageSeq { get; set; }
        public long? EditCursorId { get; set; }
        public ChatEdit? EditCursor { get; set; }
        public MessagePreview? LastPreview { get; set; }

        public ChatStateBody ToBody()
        {
            return new ChatStateBody
            {
                ChatId = ChatId,
                EditCursorId = EditCursorId,
                LastMessageSeq = LastMessageSeq,
                LastPreview = LastPreview?.ToBody()
            };
        }
    }
}
