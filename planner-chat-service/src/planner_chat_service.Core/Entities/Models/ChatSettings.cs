using planner_client_package.Entities;

namespace planner_chat_service.Core.Entities.Models
{
    public class ChatSettings
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
        public Guid AccountId { get; set; }
        public string? ChatName { get; set; }
        public string? MessageDraft { get; set; }
        public DateTime DateLastViewing { get; set; } = DateTime.UtcNow;

        public List<AccountChatSession> UserChatSessions { get; set; } = new();

        public ChatSettingsBody ToBody()
        {
            return new ChatSettingsBody()
            {
                Id = Id,
                AccountId = AccountId,
                ChatId = ChatId,
                ChatName = ChatName,
                MessageDraft = MessageDraft,
                DateLastViewing = DateLastViewing
            };
        }
    }
}