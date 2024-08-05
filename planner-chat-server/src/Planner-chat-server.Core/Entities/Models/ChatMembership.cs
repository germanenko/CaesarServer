namespace Planner_chat_server.Core.Entities.Models
{
    public class ChatMembership
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }
        public DateTime DateLastViewing { get; set; } = DateTime.UtcNow;

        public List<AccountChatSession> UserChatSessions { get; set; } = new();
    }
}