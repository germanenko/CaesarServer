using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.Entities.Models
{
    public class NotificationSettings : ModelBase
    {
        public Guid AccountId { get; set; }
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public bool NotificationsEnabled { get; set; } = true;
    }
}
