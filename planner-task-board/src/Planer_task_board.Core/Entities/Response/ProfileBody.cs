using Planer_task_board.Core.Enums;

namespace Planer_task_board.Core.Entities.Response
{
    public class ProfileBody
    {
        public Guid Id { get; set; }
        public string Identifier { get; set; }
        public string Nickname { get; set; }
        public AccountRole Role { get; set; }
        public string? UrlIcon { get; set; }
        public string? UserTag { get; set; }
    }
}