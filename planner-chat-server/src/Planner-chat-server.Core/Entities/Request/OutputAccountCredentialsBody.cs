using Planner_chat_server.Core.Enums;

namespace Planner_chat_server.Core.Entities.Request
{
    public class OutputAccountCredentialsBody
    {
        public OutputAccountCredentialsBody(string accessToken, string refreshToken)
        {
            AccessToken = $"Bearer {accessToken}";
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public AccountRole Role { get; set; }
    }
}