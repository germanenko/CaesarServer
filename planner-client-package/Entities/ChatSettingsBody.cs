using planner_client_package.Interface;
using System;

namespace planner_client_package.Entities
{
    public class ChatSettingsBody : ISyncable, IBody
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid AccountId { get; set; }
        public string ChatName { get; set; }
        public string MessageDraft { get; set; }
        public DateTime DateLastViewing { get; set; } = DateTime.UtcNow;
    }
}