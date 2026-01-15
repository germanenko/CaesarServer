using CaesarServerLibrary.Entities;
using planner_auth_service.Core.Entities.Response;

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