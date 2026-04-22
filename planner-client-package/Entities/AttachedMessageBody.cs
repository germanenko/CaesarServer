using planner_client_package.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public class AttachedMessageBody : IBody
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid MessageId { get; set; }
        public string Snapshot { get; set; }
        public DateTime AttachedAtUtc { get; set; }
    }
}
