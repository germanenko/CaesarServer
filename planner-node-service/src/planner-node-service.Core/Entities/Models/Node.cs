//using Planer_task_board.Core.Entities.Models;
using CaesarServerLibrary.Entities;
using CaesarServerLibrary.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace planner_node_service.Core.Entities.Models
{
    public class NodeSerializationBinder : DefaultSerializationBinder
    {
        private readonly Dictionary<string, Type> _typeMap = new()
        {
            ["board"] = typeof(BoardBody),
            ["column"] = typeof(ColumnBody),
            ["chat"] = typeof(ChatBody),
            ["chatmessage"] = typeof(MessageBody)
        };

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (_typeMap.ContainsKey(typeName.ToLowerInvariant()))
            {
                return _typeMap[typeName.ToLowerInvariant()];
            }

            return base.BindToType(assemblyName, typeName);
        }
    }

    public class Node : ModelBase
    {
        public NodeType Type { get; set; }
        public string Name { get; set; }
        public string? Props { get; set; }
        public string BodyJson { get; set; }
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

        public NodeBody ToNodeBodyFromJson()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = new NodeSerializationBinder()
            };

            return JsonConvert.DeserializeObject<NodeBody>(BodyJson, settings);
        }
    }
}
