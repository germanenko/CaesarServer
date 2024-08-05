namespace Planner_chat_server.Core.Entities.Events
{
    public class CreateChatEvent
    {
        public Guid ChatId { get; set; }
        public IEnumerable<ChatMembership> Participants { get; set; } = new List<ChatMembership>();
    }

    public class ChatMembership
    {
        public Guid AccountId { get; set; }
        public Guid ChatMembershipId { get; set; }
    }
}