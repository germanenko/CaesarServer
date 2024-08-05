namespace Planer_mailbox_service.Core.Entities.Request
{
    public class CreateMailMessageDto
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }
}