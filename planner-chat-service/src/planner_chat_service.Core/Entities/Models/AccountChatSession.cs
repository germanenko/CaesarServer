namespace planner_chat_service.Core.Entities.Models
{
    public class AccountChatSession
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }

        public ChatSettings ChatSetting { get; set; }
        public Guid ChatSettingId { get; set; }

        public DateTime DateLastViewing { get; set; } = DateTime.UtcNow;
    }
}