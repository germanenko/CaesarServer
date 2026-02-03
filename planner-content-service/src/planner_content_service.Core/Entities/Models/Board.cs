using planner_client_package.Entities;

namespace planner_content_service.Core.Entities.Models
{
    public class Board : Node
    {
        public string HexColor { get; set; } = "FFFFFF";

        public BoardBody ToBoardBody()
        {
            return new BoardBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type
            };
        }

        public override NodeBody ToNodeBody()
        {
            return new BoardBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type
            };
        }
    }
}