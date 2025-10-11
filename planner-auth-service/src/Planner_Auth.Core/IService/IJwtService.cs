using Planner_Auth.Core.Entities.Response;

namespace Planner_Auth.Core.IService
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