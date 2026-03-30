using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class MessagePreviewBody
    {
        public Guid MessageId { get; set; }
        public Guid AuthorId { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; }
    }
}
