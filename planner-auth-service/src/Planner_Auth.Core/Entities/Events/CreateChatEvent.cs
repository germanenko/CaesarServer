namespace Planner_Auth.Core.Entities.Events
{
    public class CreateChatEvent
    {
        public Guid ChatId { get; set; }
        public IEnumerable<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();

        public class ChatParticipant
        {
            public Guid AccountId { get; set; }
            public Guid ChatMembershipId { get; set; }
            public IEnumerable<Guid> SessionIds { get; set; } = new List<Guid>();
        }
    }
}