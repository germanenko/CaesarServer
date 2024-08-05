namespace Planner_chat_server.Core.Entities.Request
{
    public class TokenPayload
    {
        public Guid AccountId { get; set; }
        public Guid SessionId { get; set; }
        public string Role { get; set; }
    }
}