using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.Entities.Models
{
    public class History : ModelBase
    {
        public Guid TrackableId { get; set; }
        public TrackableEntity Trackable { get; set; }
        public DateTime Date { get; set; }
        public Guid ActorId { get; set; }
        public ActionType Action { get; set; }
    }
}
