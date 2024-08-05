namespace Planner_Auth.Core.Entities.Events
{
    public class CreateChatResponseEvent
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