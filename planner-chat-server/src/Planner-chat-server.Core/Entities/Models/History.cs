namespace Planner_chat_server.Core.Entities.Models
{
    public class History
    {
        public Guid Id { get; set; }
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid UpdatedBy { get; set; }
    }
}
