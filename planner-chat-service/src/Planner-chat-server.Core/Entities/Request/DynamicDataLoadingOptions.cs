using System.ComponentModel.DataAnnotations;

namespace Planner_chat_server.Core.Entities.Request
{
    public class DynamicDataLoadingOptions
    {
        [Range(1, int.MaxValue)]
        public int Count { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int LoadPosition { get; set; } = 0;
    }
}