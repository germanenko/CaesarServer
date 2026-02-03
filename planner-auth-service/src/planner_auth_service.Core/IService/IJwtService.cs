using planner_auth_service.Core.Entities.Response;
using planner_common_package.Entities;
using planner_server_package.Entities;

namespace planner_auth_service.Core.IService
{
    public interface IJwtService
    {
        OutputAccountCredentialsBody GenerateDefaultTokenPair(TokenPayload tokenPayload);
        TokenPayload GetTokenPayload(string token);
        string GeneratePasswordResetToken(string userId);
        bool ValidatePasswordResetToken(string token, string expectedEmail = null);
        PasswordResetTokenPayload GetPasswordResetTokenPayload(string token);
        DateTime GetTokenExpiration(string token);
        void InvalidatePasswordResetToken(string token);
    }
}