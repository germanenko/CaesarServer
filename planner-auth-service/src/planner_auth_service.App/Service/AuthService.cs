using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using planner_auth_service.Core.Entities.Response;
using planner_auth_service.Core.IRepository;
using planner_auth_service.Core.IService;
using planner_server_package.Entities;
using planner_server_package.Enums;
using planner_server_package.Events;
using System.Net;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace planner_auth_service.App.Service
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IJwtService _jwtService;
        private readonly IHashPasswordService _hashPasswordService;
        private readonly INotifyService _notifyService;
        private readonly ILogger<AuthService> _logger;

        public AuthService
        (
            IAccountRepository accountRepository,
            IJwtService jwtService,
            IHashPasswordService hashPasswordService,
            INotifyService notifyService,
            ILogger<AuthService> logger
        )
        {
            _accountRepository = accountRepository;
            _jwtService = jwtService;
            _hashPasswordService = hashPasswordService;
            _notifyService = notifyService;
            _logger = logger;
        }

        public async Task<ServiceResponse<OutputAccountCredentialsBody>> RestoreToken(
            string refreshToken,
            string deviceId,
            DeviceTypeId deviceTypeId)
        {
            var isValidDeviceId = IsValidDeviceId(deviceId, deviceTypeId);
            if (!isValidDeviceId)
            {
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "DeviceId is not correct format" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            var session = await _accountRepository.GetSessionByTokenAndAccount(refreshToken);
            if (session == null)
            {
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Session not found" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound
                };
            }

            var account = session.Account;
            var accountCredentials = await UpdateToken(account.RoleName, account.Id, session.Id);

            return new ServiceResponse<OutputAccountCredentialsBody>
            {
                Body = accountCredentials,
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ServiceResponse<OutputAccountCredentialsBody>> SignIn(SignInBody body, AuthenticationProvider provider)
        {
            var isValidIdentifier = IsValidAuthenticationIdentifier(body.Identifier, body.Method);
            if (!isValidIdentifier)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Identifier is not correct format" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };

            var isValidDeviceId = IsValidDeviceId(body.DeviceId, body.DeviceTypeId);
            if (!isValidDeviceId)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "DeviceId is not correct format" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };

            var account = await _accountRepository.GetAsync(body.Identifier);
            if (account == null)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Account not found" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound
                };

            //if (account.AuthorizationProvider != provider.ToString())
            //    return new ServiceResponse<OutputAccountCredentialsBody>
            //    {
            //        Errors = new string[] { "Account created by another provider" },
            //        IsSuccess = false,
            //        StatusCode = HttpStatusCode.Conflict
            //    };

            var inputPasswordHash = _hashPasswordService.HashPassword(body.Password);
            if (account.PasswordHash != inputPasswordHash)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Password is not correct" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };

            var sessionId = await CreateSession(body.DeviceId, account.Id);
            var outputAccountCredentials = await UpdateToken(account.RoleName, account.Id, sessionId.Value);
            return new ServiceResponse<OutputAccountCredentialsBody>
            {
                Body = outputAccountCredentials,
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<HttpStatusCode> CreateMailCredentials(string email, string? access_token, string? refresh_token, EmailProvider provider)
        {
            var account = await _accountRepository.GetAsync(email);
            if (account == null)
                return HttpStatusCode.NotFound;

            if (refresh_token == null || access_token == null)
                return HttpStatusCode.BadRequest;

            var body = new CreateAccountMailCredentialsEvent
            {
                Email = email,
                AccessToken = access_token,
                RefreshToken = refresh_token,
                AccountId = account.Id,
                EmailProvider = provider
            };
            _notifyService.Publish(body, PublishEvent.CreateAccountMailCredentials);
            return HttpStatusCode.OK;
        }

        public async Task<ServiceResponse<OutputAccountCredentialsBody>> SignUp(
            SignUpBody body,
            string rolename,
            AuthenticationProvider provider)
        {
            var isValidIdentifier = IsValidAuthenticationIdentifier(body.Identifier, body.Method);
            if (!isValidIdentifier)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Identifier is not correct format" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };

            var isValidDeviceId = IsValidDeviceId(body.DeviceId, body.DeviceTypeId);
            if (!isValidDeviceId)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "DeviceId is not correct format" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };

            var account = await _accountRepository.AddAsync(
                body.Identifier,
                body.Nickname,
                _hashPasswordService.HashPassword(body.Password),
                body.DeviceId,
                body.DeviceTypeId,
                body.Method,
                rolename,
                provider);
            if (account == null)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Account exists" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Conflict
                };

            var session = await CreateSession(body.DeviceId, account.Id);
            var tokenPair = await UpdateToken(rolename, account.Id, session.Value);
            return new ServiceResponse<OutputAccountCredentialsBody>
            {
                Body = tokenPair,
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            };
        }

        private async Task<OutputAccountCredentialsBody> UpdateToken(string rolename, Guid accountId, Guid sessionId)
        {
            var tokenPayload = new TokenPayload
            {
                Role = rolename,
                AccountId = accountId,
                SessionId = sessionId
            };

            var accountCredentials = _jwtService.GenerateDefaultTokenPair(tokenPayload);
            accountCredentials.RefreshToken = await _accountRepository.UpdateTokenAsync(accountCredentials.RefreshToken, tokenPayload.SessionId);
            return accountCredentials;
        }

        private bool IsValidDeviceId(string deviceId, DeviceTypeId deviceTypeId)
        {
            switch (deviceTypeId)
            {
                case DeviceTypeId.AndroidId:
                    var regex = new Regex("^[0-9a-fA-F]{16}$");
                    return regex.IsMatch(deviceId);

                case DeviceTypeId.UUID:
                    return Guid.TryParse(deviceId, out var uuid);
            }

            return false;
        }

        private bool IsValidAuthenticationIdentifier(string identifier, DefaultAuthenticationMethod method)
        {
            return method switch
            {
                DefaultAuthenticationMethod.Email => IsValidEmail(identifier),
                DefaultAuthenticationMethod.Phone => IsValidPhoneNumber(identifier),
                _ => false,
            };
        }

        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            return emailRegex.IsMatch(email);
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            var phoneRegex = new Regex(@"^\+?\d+$");
            return phoneRegex.IsMatch(phoneNumber);
        }

        public async Task<bool> AccountAuthorizedByProvider(string identifier, AuthenticationProvider provider)
        {
            var account = await _accountRepository.GetAsync(identifier);
            return account != null && account.AuthorizationProvider == provider.ToString();
        }

        private async Task<Guid?> CreateSession(string deviceId, Guid accountId)
        {
            var account = await _accountRepository.GetAsync(accountId);
            if (account == null)
                return null;

            var session = await _accountRepository.AddSessionAsync(deviceId, account);
            if (session != null)
            {
                var initChatEvent = new InitChatEvent
                {
                    AccountId = account.Id,
                    SessionIds = new List<Guid> { session.Id }
                };
                _notifyService.Publish(initChatEvent, PublishEvent.InitChat);
            }
            else
                session = await _accountRepository.GetSessionAsync(account.Id, deviceId);
            return session?.Id;
        }

        public async Task<ServiceResponse<string>> AddGoogleToken(GoogleTokenBody token, Guid accountId)
        {
            var result = await _accountRepository.AddAsync(token, accountId);

            if (result == null)
            {
                return new ServiceResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }

            return new ServiceResponse<string>
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Body = result.AccessToken
            };
        }

        public async Task<ServiceResponse<string>> GetGoogleToken(Guid accountId)
        {
            var result = await _accountRepository.GetGoogleTokenAsync(accountId);
            if (result != null)
            {
                return new ServiceResponse<string>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Body = result.AccessToken
                };
            }
            else
            {
                return new ServiceResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };
            }
        }

        private async Task<ServiceResponse<OutputAccountCredentialsBody>> SignInWithHash(SignInBody body, AuthenticationProvider provider)
        {
            var isValidIdentifier = IsValidAuthenticationIdentifier(body.Identifier, body.Method);
            if (!isValidIdentifier)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Identifier is not correct format" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };

            var isValidDeviceId = IsValidDeviceId(body.DeviceId, body.DeviceTypeId);
            if (!isValidDeviceId)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { $"DeviceId is not correct format" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };

            var account = await _accountRepository.GetAsync(body.Identifier);
            if (account == null)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Account not found" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound
                };


            if (account.AuthorizationProvider != "Default" && account.AuthorizationProvider != provider.ToString())
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Account created by another provider" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Conflict
                };
            else
                await _accountRepository.UpdateAuthorizationProviderAsync(account.Id, provider.ToString());

            if (account.PasswordHash != body.Password)
                return new ServiceResponse<OutputAccountCredentialsBody>
                {
                    Errors = new string[] { "Password is not correct" },
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest
                };

            var sessionId = await CreateSession(body.DeviceId, account.Id);
            var outputAccountCredentials = await UpdateToken(account.RoleName, account.Id, sessionId.Value);
            return new ServiceResponse<OutputAccountCredentialsBody>
            {
                Body = outputAccountCredentials,
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ServiceResponse<OutputAccountCredentialsBody>> GoogleAuth(
            GoogleTokenBody token, DeviceTypeId deviceTypeId, string deviceId)
        {
            var userCredential = GoogleCredential.FromAccessToken(token.AccessToken);

            var oauthService = new Google.Apis.Oauth2.v2.Oauth2Service(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = userCredential
                });

            var userInfo = await oauthService.Userinfo.Get().ExecuteAsync();

            var account = await _accountRepository.GetAsync(userInfo.Email);

            ServiceResponse<OutputAccountCredentialsBody> tokenPair;

            if (account == null)
            {
                tokenPair = await SignUp(new SignUpBody()
                {
                    Identifier = userInfo.Email,
                    Nickname = userInfo.Name,
                    Method = DefaultAuthenticationMethod.Email,
                    Password = Guid.NewGuid().ToString(),
                    DeviceId = deviceId,
                    DeviceTypeId = deviceTypeId
                },
                "User",
                AuthenticationProvider.Google);

                if (!tokenPair.IsSuccess)
                {
                    return tokenPair;
                }

                var accountId = _jwtService.GetTokenPayload(tokenPair.Body.AccessToken).AccountId;

                await AddGoogleToken(token, accountId);

                await NotifyAboutTempPassword(userInfo.Email, accountId);
            }
            else
            {
                tokenPair = await SignInWithHash(new SignInBody()
                {
                    Identifier = userInfo.Email,
                    Method = DefaultAuthenticationMethod.Email,
                    Password = account.PasswordHash,
                    DeviceId = deviceId,
                    DeviceTypeId = deviceTypeId
                },
                AuthenticationProvider.Google);
            }

            await _accountRepository.AddAsync(token, account.Id);
            return tokenPair;
        }

        public async Task<bool> NotifyAboutTempPassword(string email, string accessToken)
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri("http://planner-email-service:80/api/"),
            };

            var token = _jwtService.GetTokenPayload(accessToken);

            var s = $"{{ \"subject\": \"Временный пароль\", \"content\": \"Вашему аккаунту присвоен временный пароль! Смените его по ссылке: {GenerateResetLink(token.AccountId)}\"}}";
            var content = new StringContent(s, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await client.PostAsync("message/serviceEmail" + $"?email={email}", content);

            string responseString = response.StatusCode.ToString();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GenerateResetLink(Guid accountId)
        {
            var token = _jwtService.GeneratePasswordResetToken(accountId.ToString());

            var resetLink = $"https://busfy.ru/auth/reset-password?token={Uri.EscapeDataString(token)}";

            return resetLink;
        }

        public async Task<ServiceResponse<string>> ResetPassword(Guid accountId, string newPassword)
        {
            var account = await _accountRepository.GetAsync(accountId);

            var newPasswordHash = _hashPasswordService.HashPassword(newPassword);

            var result = await _accountRepository.ChangePassword(account, newPasswordHash);

            if (result != null)
            {
                return new ServiceResponse<string>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Body = "Пароль успешно изменён"
                };
            }
            else
            {
                return new ServiceResponse<string>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.BadRequest,
                    Body = "Ошибка"
                };
            }
        }

        public async Task<ServiceResponse<string>> ChangePassword(Guid accountId, string oldPassword, string newPassword)
        {
            var account = await _accountRepository.GetAsync(accountId);

            var oldPasswordHash = _hashPasswordService.HashPassword(oldPassword);

            if (oldPasswordHash != account.PasswordHash)
            {
                return new ServiceResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Body = "Старый пароль неверный"
                };
            }

            return await ResetPassword(accountId, newPassword);
        }
    }
}