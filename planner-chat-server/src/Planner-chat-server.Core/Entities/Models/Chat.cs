namespace Planner_chat_server.Core.Entities.Models
{
    public class Chat
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string? Image { get; set; }
        public string Type { get; set; }
        public Guid? TaskId { get; set; }

        public List<ChatMessage> Messages { get; set; } = new();
        public List<ChatMembership> ChatMemberships { get; set; } = new();
    }
}