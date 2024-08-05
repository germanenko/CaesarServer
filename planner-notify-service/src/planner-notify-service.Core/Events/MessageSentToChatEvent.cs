namespace planner_notify_service.Core.Events
{
    public class MessageSentToChatEvent
    {
        public IEnumerable<Guid> AccountIds { get; set; }
        public IEnumerable<AccountSessions> AccountSessions { get; set; } = new List<AccountSessions>();
        public byte[] Message { get; set; }
    }

    public class AccountSessions
    {
        public Guid AccountId { get; set; }
        public IEnumerable<Guid> SessionIds { get; set; } = new List<Guid>();
    }
}