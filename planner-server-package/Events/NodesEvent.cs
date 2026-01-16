using planner_server_package.Entities;
using System;
using System.Collections.Generic;

namespace planner_server_package.Events
{
    public class NodesEvent
    {
        public TokenPayload TokenPayload { get; set; }
        public List<NodeBody> Nodes { get; set; }
    }
}