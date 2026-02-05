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
        public string? OldVersion { get; set; }
        public string? NewVersion { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid UpdatedById { get; set; }
        public ActionType Action { get; set; }
    }
}
