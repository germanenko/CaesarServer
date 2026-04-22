using planner_client_package.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class ReadStateBody : IBody
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid AccountId { get; set; }
        public DateTime LastReadAtUtc { get; set; }
        public int AttachedUnreadCount { get; set; }
        public string AttachedLastPreview { get; set; }
    }
}
