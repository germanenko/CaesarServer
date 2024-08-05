namespace Planner_chat_server.Core.Entities.Response
{
    public class ChatBody
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public int CountOfUnreadMessages { get; set; }
        public MessageBody? LastMessage { get; set; }
        public bool IsSyncedReadStatus { get; set; }

        public List<Guid> ParticipantIds { get; set; } = new();
    }
}