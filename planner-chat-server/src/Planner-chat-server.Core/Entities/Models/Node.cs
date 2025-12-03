using Planner_chat_server.Core.Entities.Response;
using Planner_chat_server.Core.Enums;

namespace Planner_chat_server.Core.Entities.Models
{
    public class Node : ModelBase
    {
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string? Props { get; set; }

        public NodeBody ToNodeBody()
        {
            return new NodeBody()
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type
            };
        }
    }
}
