using Planer_task_board.Core.Entities.Response;
using Planer_task_board.Core.Enums;
using System.Text.Json.Serialization;

namespace Planer_task_board.Core.Entities.Models
{
    [JsonDerivedType(typeof(Board), "board")]
    [JsonDerivedType(typeof(Column), "column")]
    [JsonDerivedType(typeof(TaskModel), "task")]
    [JsonDerivedType(typeof(Chat), typeDiscriminator: "chat")]
    [JsonDerivedType(typeof(ChatMessage), typeDiscriminator: "chatMessage")]
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
