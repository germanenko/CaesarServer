namespace Planner_Auth.Core.Entities.Events
{
    public class InitChatEvent
    {
        public Guid AccountId { get; set; }
        public IEnumerable<Guid> SessionIds { get; set; } = new List<Guid>();
    }
}