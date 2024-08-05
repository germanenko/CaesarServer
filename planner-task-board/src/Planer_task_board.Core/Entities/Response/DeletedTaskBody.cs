namespace Planer_task_board.Core.Entities.Response
{
    public class DeletedTaskBody
    {
        public Guid Id { get; set; }
        public string ExistBeforeDate { get; set; }

        public TaskBody Task { get; set; }
    }
}