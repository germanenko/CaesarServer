namespace Planner_chat_server.Core.Entities.Response
{
    public class AccountSessions
    {
        public Guid AccountId { get; set; }
        public List<Guid> SessionIds { get; set; } = new();
    }
}