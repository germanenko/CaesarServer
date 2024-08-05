namespace Planer_mailbox_service.Core.Entities.Response
{
    public class EmailMessageInfoDto
    {
        public string Subject { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public DateTime Date { get; set; }
        public string Body { get; set; }
        public int Index { get; set; }

        public EmailMessageInfoDto(string subject, string from, string to, DateTime date, string body, int index)
        {

            Subject = subject;
            From = from;
            To = to;
            Date = date;
            Body = body;
            Index = index;
        }
    }
}