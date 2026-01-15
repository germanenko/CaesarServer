namespace planner_content_service.Core.Entities.Models
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
    }
}