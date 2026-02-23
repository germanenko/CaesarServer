using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Entities
{
    public class NotificationBody
    {
        public Guid AccountId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public NotificationType Type { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public NotificationBody(Guid accountId, string title, string content, NotificationType type, Dictionary<string, string> data)
        {
            AccountId = accountId;
            Title = title;
            Content = content;
            Type = type;
            Data = data;
        }
    }
}
