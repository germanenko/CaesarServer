using planner_node_service.Core.Enums;

namespace planner_node_service.Core.Entities.Response
{
    public class NodeBody
    {
        public Guid Id { get; set; }
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string? Props { get; set; }
    }
}
