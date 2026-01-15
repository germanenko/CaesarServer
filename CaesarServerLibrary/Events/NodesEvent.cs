using CaesarServerLibrary.Entities;
using System;
using System.Collections.Generic;

namespace CaesarServerLibrary.Events
{
    public class NodesEvent
    {
        public TokenPayload TokenPayload { get; set; }
        public List<NodeBody> Nodes { get; set; }
    }
}