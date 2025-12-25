using CaesarServerLibrary.Enums;

namespace Planer_task_board.Core.Entities.Models
{
    public class PublicationStatusModel : ModelBase
    {
        public Guid NodeId { get; set; }
        public Node Node { get; set; }
        public PublicationStatus Status { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
