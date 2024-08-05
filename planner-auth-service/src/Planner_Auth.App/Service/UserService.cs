using System.Net;
using Planner_Auth.Core.Entities.Response;
using Planner_Auth.Core.IRepository;
using Planner_Auth.Core.IService;

namespace Planner_Auth.App.Service
{
    public class UserService : IUserService
    {
        private readonly IAccountRepository _accountRepository;

        public UserService(
            IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public async Task<ServiceResponse<ProfileBody>> GetProfile(Guid accountId)
        {
            var account = await _accountRepository.GetAsync(accountId);
            if (account == null)
                return new ServiceResponse<ProfileBody>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new[] { "Account not found" }
                };

            return new ServiceResponse<ProfileBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = account.ToProfileBody()
            };
        }

        public async Task<HttpStatusCode> ChangeAccountTag(Guid accountId, string tag)
        {
            var account = await _accountRepository.GetAsync(accountId);
            if (account == null)
                return HttpStatusCode.NotFound;

            account = await _accountRepository.UpdateAccountTagAsync(accountId, tag);
            return account == null ? HttpStatusCode.BadRequest : HttpStatusCode.OK;
        }

        public async Task<ServiceResponse<ProfileBody>> GetProfileByTag(string tag)
        {
            var account = await _accountRepository.GetAccountByTagAsync(tag);
            if (account == null)
                return new ServiceResponse<ProfileBody>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new[] { "Account not found" }
                };

            return new ServiceResponse<ProfileBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = account.ToProfileBody()
            };
        }

        public async Task<ServiceResponse<IEnumerable<ProfileBody>>> GetAllProfilesByPatternTag(string patternTag)
        {
            var accounts = await _accountRepository.GetAccountsByPatternAccountTag(patternTag);
            return new ServiceResponse<IEnumerable<ProfileBody>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = accounts.Select(a => a.ToProfileBody())
            };
        }

        public async Task<ServiceResponse<ProfileBody>> GetProfile(string identifier)
        {
            var account = await _accountRepository.GetAsync(identifier);
            if (account == null)
                return new ServiceResponse<ProfileBody>
                {
                    StatusCode = HttpStatusCode.NotFound,
                    IsSuccess = false,
                    Errors = new[] { "Account not found" }
                };

            return new ServiceResponse<ProfileBody>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = account.ToProfileBody()
            };
        }

        public async Task<ServiceResponse<IEnumerable<ProfileBody>>> GetAllProfilesByPatternIdentifier(string patternIdentifier)
        {
            var accounts = await _accountRepository.GetAccountsByPatternIdentifier(patternIdentifier);
            return new ServiceResponse<IEnumerable<ProfileBody>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = accounts.Select(a => a.ToProfileBody())
            };
        }

        public async Task<ServiceResponse<IEnumerable<ProfileBody>>> GetAllProfiles(IEnumerable<Guid> accountIds)
        {
            var accounts = await _accountRepository.GetAccountsAsync(accountIds);
            return new ServiceResponse<IEnumerable<ProfileBody>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = accounts.Select(a => a.ToProfileBody())
            };
        }
    }
}