using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace planner_client_package.Entities.WebSockets
{
    public class WebSocketMessage
    {
        public MessageType MessageType { get; set; }
        public JsonElement Message { get; set; }
    }
}
