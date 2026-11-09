using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace planner_client_package.Entities.WebSockets
{
    [Serializable]
    public class WebSocketMessage
    {
        public WebSocketMessageKind MessageKind { get; set; }
        public string MessageJson { get; set; }
    }
}
