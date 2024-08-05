namespace Planner_chat_server.Core.Entities.Models
{
    public class AccountChatSession
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }

        public ChatMembership ChatMembership { get; set; }
        public Guid ChatMembershipId { get; set; }

        public DateTime DateLastViewing { get; set; } = DateTime.UtcNow;
    }
}