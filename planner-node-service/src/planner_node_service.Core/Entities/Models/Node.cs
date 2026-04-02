//using Planer_task_board.Core.Entities.Models;
using Newtonsoft.Json.Serialization;
using planner_client_package.Entities;
using planner_common_package.Enums;
using System.Text.Json;

namespace planner_node_service.Core.Entities.Models
{
    public class Node
    {
        public Guid Id { get; set; }
        //public long CursorId { get; set; }
        //public ContentLog Cursor { get; set; }
        public long Version { get; set; }
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string? Props { get; set; }

        public SyncKind SyncKind { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            if (obj.GetType() != typeof(Node))
            {
                return false;
            }

            var node = obj as Node;

            if (node == null) return false;

            if (node.Name != Name || node.Props != Props)
            {
                return false;
            }

            return true;
        }

        public NodeBody ToNodeBody()
        {
            return new NodeBody()
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type,
                Version = Version,
                SyncKind = SyncKind
            };
        }
    }
}
