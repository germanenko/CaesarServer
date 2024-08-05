using Planer_task_board.Core.Entities.Response;

namespace Planer_task_board.Core.Entities.Models
{
    public class DeletedTask
    {
        public Guid Id { get; set; }
        public DateTime ExistBeforeDate { get; set; }

        public TaskModel Task { get; set; }
        public Guid TaskId { get; set; }

        public DeletedTaskBody ToDeletedTaskBody()
        {
            return new DeletedTaskBody
            {
                Id = Id,
                ExistBeforeDate = ExistBeforeDate.ToString("s"),
                Task = Task.ToTaskBody()
            };
        }
    }
}