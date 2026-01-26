using planner_client_package.Entities;

namespace planner_content_service.Core.Entities.Models
{
    public class Column : Node
    {
        public ColumnBody ToColumnBody()
        {
            return new ColumnBody
            {
                Id = Id,
                Name = Name,
                Props = Props,
                Type = Type
            };
        }
    }
}