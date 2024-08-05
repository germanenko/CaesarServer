using System.Net;
using Planner_Auth.Core.Entities.Response;

namespace Planner_Auth.Core.IService
{
    public interface IUserService
    {
        Task<ServiceResponse<ProfileBody>> GetProfile(string identifier);
        Task<ServiceResponse<IEnumerable<ProfileBody>>> GetAllProfilesByPatternIdentifier(string patternIdentifier);
        Task<ServiceResponse<IEnumerable<ProfileBody>>> GetAllProfilesByPatternTag(string patternTag);
        Task<ServiceResponse<IEnumerable<ProfileBody>>> GetAllProfiles(IEnumerable<Guid> accountIds);
        Task<ServiceResponse<ProfileBody>> GetProfileByTag(string tag);
        Task<HttpStatusCode> ChangeAccountTag(Guid accountId, string tag);
        Task<ServiceResponse<ProfileBody>> GetProfile(Guid accountId);
    }
}