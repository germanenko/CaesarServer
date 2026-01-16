using planner_server_package.Entities;
using planner_server_package.Enums;
using planner_auth_service.Core.Entities.Response;
using System.Net;

namespace planner_auth_service.Core.IService
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
        Task<ServiceResponse<string>> ResetPassword(Guid accountId, string newPassword);
        Task<ServiceResponse<string>> ChangePassword(Guid accountId, string oldPassword, string newPassword);
    }
}