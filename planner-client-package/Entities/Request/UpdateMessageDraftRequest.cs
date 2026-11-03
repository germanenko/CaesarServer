using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.Request
{
    public record UpdateMessageDraftRequest
    {
        public Guid ChatId { get; set; }
        public string Draft { get; set; }
        public UpdateMessageDraftRequest(Guid chatId, string draft)
        {
            ChatId = chatId;
            Draft = draft;
        }
    }
}
