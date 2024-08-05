using System.Text.Json.Serialization;

namespace Planer_task_board.Core.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TaskType
    {
        Meeting,
        Task,
        Reminder,
        Inform
    }
}