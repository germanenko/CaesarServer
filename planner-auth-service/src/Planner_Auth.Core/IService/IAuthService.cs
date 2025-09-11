using System.Net;
using Planner_Auth.Core.Entities.Request;
using Planner_Auth.Core.Entities.Response;
using Planner_Auth.Core.Enums;

namespace Planner_Auth.Core.IService
{
    public interface IAuthService
    {
        Task<ServiceResponse<OutputAccountCredentialsBody>> SignUp(SignUpBody body, string rolename, AuthenticationProvider provider);
        Task<ServiceResponse<OutputAccountCredentialsBody>> SignIn(SignInBody body, AuthenticationProvider provider);
        Task<ServiceResponse<OutputAccountCredentialsBody>> RestoreToken(string refreshToken, string deviceId, DeviceTypeId deviceTypeId);
        Task<HttpStatusCode> CreateMailCredentials(string email, string? access_token, string? refresh_token, EmailProvider provider);
        Task<ServiceResponse<string>> AddGoogleToken(GoogleTokenBody token, Guid accountId);
        Task<ServiceResponse<string>> GetGoogleToken(Guid accountId);
        Task<ServiceResponse<OutputAccountCredentialsBody>> GoogleAuth(GoogleTokenBody token, DeviceTypeId deviceTypeId, string deviceId);
    }
}