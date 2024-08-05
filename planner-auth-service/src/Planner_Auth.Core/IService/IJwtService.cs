using Planner_Auth.Core.Entities.Response;

namespace Planner_Auth.Core.IService
{
    public interface IJwtService
    {
        OutputAccountCredentialsBody GenerateDefaultTokenPair(TokenPayload tokenPayload);
        TokenPayload GetTokenPayload(string token);
    }
}