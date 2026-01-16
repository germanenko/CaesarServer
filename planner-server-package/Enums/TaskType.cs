using System.Text.Json.Serialization;

namespace planner_server_package.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TaskType
    {
        Meeting,
        Task,
        Reminder,
        Information
    }
}