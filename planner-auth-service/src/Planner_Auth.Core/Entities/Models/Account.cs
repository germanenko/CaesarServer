using Planner_Auth.Core.Entities.Response;
using Planner_Auth.Core.Enums;

namespace Planner_Auth.Core.Entities.Models
{
    public class Account
    {
        public Guid Id { get; set; }

        public string Identifier { get; set; }
        public string Nickname { get; set; }
        public string PasswordHash { get; set; }
        public string? RestoreCode { get; set; }
        public string RoleName { get; set; }
        public DateTime? RestoreCodeValidBefore { get; set; }
        public bool WasPasswordResetRequest { get; set; }
        public string? Image { get; set; }
        public string? Tag { get; set; }
        public string AuthenticationMethod { get; set; }
        public string AuthorizationProvider { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<AccountSession> Sessions { get; set; } = new();

        public ProfileBody ToProfileBody()
        {
            return new ProfileBody
            {
                Id = Id,
                Nickname = Nickname,
                Role = Enum.Parse<AccountRole>(RoleName),
                UrlIcon = string.IsNullOrEmpty(Image) ? null : $"{Constants.WebUrlToProfileImage}/{Image}",
                UserTag = Tag,
                Identifier = Identifier,
                IdentifierType = Enum.Parse<DefaultAuthenticationMethod>(AuthenticationMethod)
            };
        }

    }
}