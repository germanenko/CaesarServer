using Planner_Auth.Core.Enums;

namespace Planner_Auth.Core.Entities.Response
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