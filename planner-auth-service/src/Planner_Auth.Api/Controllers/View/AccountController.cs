using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Planner_Auth.Core.Entities.Request;
using Planner_Auth.Core.Enums;
using Planner_Auth.Core.IService;

namespace Planner_Auth.Api.Controllers.View
{
    [Route("")]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IMailRuTokenService _mailRuTokenService;
        private readonly ILogger<AccountController> _logger;



        public AccountController(
            IAuthService authService,
            IMailRuTokenService mailRuTokenService,
            ILogger<AccountController> logger
        )
        {
            _authService = authService;
            _mailRuTokenService = mailRuTokenService;
            _logger = logger;
        }

        [HttpGet("mail-login")]
        public IActionResult MailLogin([FromQuery(Name = "deviceId"), Required] string deviceId)
        {
            var authorizationUrl = _mailRuTokenService.GetAuthorizationUrl();
            var deviceTypeId = GetDeviceType(deviceId);
            if (deviceTypeId == null)
                return BadRequest();

            HttpContext.Session.SetString("deviceId", deviceId);
            HttpContext.Session.SetString("deviceTypeId", deviceTypeId.ToString());
            return Redirect(authorizationUrl);
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin([FromQuery(Name = "deviceId"), Required] string deviceId)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse"),
                Items = { { "access_type", "offline" },
                         { "prompt", "consent" }
                        }
            };

            var deviceTypeId = GetDeviceType(deviceId);
            if (deviceTypeId == null)
                return BadRequest();

            HttpContext.Session.SetString("deviceId", deviceId);
            HttpContext.Session.SetString("deviceTypeId", deviceTypeId.ToString());

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("singin-google")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return Unauthorized();

            var claims = result.Principal.Claims;
            var email = claims.First(e => e.Type == ClaimTypes.Email).Value;
            var name = claims.First(e => e.Type == ClaimTypes.Name).Value;
            var nameIdentifier = claims.First(e => e.Type == ClaimTypes.NameIdentifier).Value;

            var session = HttpContext.Session;
            var deviceTypeId = session.GetString("deviceTypeId");
            var deviceId = session.GetString("deviceId");

            var access_token = result.Properties.GetTokenValue("access_token");
            var refreshToken = result.Properties.GetTokenValue("refresh_token");
            _logger.LogInformation($"access_token: {access_token}, refreshToken: {refreshToken}");

            var mailCreadentials = await _authService.CreateMailCredentials(email, access_token, refreshToken, EmailProvider.Gmail);
            var signInBody = new SignInBody
            {
                Identifier = email,
                Method = DefaultAuthenticationMethod.Email,
                Password = nameIdentifier,
                DeviceId = deviceId,
                DeviceTypeId = Enum.Parse<DeviceTypeId>(deviceTypeId)
            };

            var signInResponse = await _authService.SignIn(signInBody, AuthenticationProvider.Google);

            if (signInResponse.IsSuccess)
                return StatusCode((int)signInResponse.StatusCode, signInResponse.Body);

            if (signInResponse.StatusCode == HttpStatusCode.Conflict)
                return StatusCode((int)signInResponse.StatusCode, signInResponse.Errors);

            var signUpBody = new SignUpBody
            {
                Identifier = email,
                Method = DefaultAuthenticationMethod.Email,
                Nickname = name,
                Password = nameIdentifier,
                DeviceId = deviceId,
                DeviceTypeId = Enum.Parse<DeviceTypeId>(deviceTypeId)
            };

            var response = await _authService.SignUp(signUpBody, AccountRole.User.ToString(), AuthenticationProvider.Google);
            if (response.IsSuccess)
            {
                await _authService.CreateMailCredentials(email, access_token, refreshToken, EmailProvider.Gmail);
                return StatusCode((int)response.StatusCode, response.Body);
            }
            return StatusCode((int)response.StatusCode, response.Errors);
        }

        [HttpGet("signin-mail")]
        public async Task<IActionResult> MailResponse(string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Authorization code is missing.");

            var tokenPairResponse = await _mailRuTokenService.GetTokenAsync(code);
            if (tokenPairResponse == null)
                return BadRequest("Failed to retrieve access token.");

            var session = HttpContext.Session;
            var deviceTypeId = session.GetString("deviceTypeId");
            var deviceId = session.GetString("deviceId");

            var userInfo = await _mailRuTokenService.GetUserInfo(tokenPairResponse.AccessToken);
            if (userInfo == null)
                return BadRequest("Получить профиль пользователя не получается");

            var mailCreadentials = await _authService.CreateMailCredentials(userInfo.Email, tokenPairResponse.AccessToken, tokenPairResponse.RefreshToken, EmailProvider.MailRu);
            var signInBody = new SignInBody
            {
                Identifier = userInfo.Email,
                Method = DefaultAuthenticationMethod.Email,
                Password = userInfo.FirstName,
                DeviceId = deviceId,
                DeviceTypeId = Enum.Parse<DeviceTypeId>(deviceTypeId)
            };

            var signInResponse = await _authService.SignIn(signInBody, AuthenticationProvider.MailRu);

            if (signInResponse.IsSuccess)
                return StatusCode((int)signInResponse.StatusCode, signInResponse.Body);

            if (signInResponse.StatusCode == HttpStatusCode.Conflict)
                return StatusCode((int)signInResponse.StatusCode, signInResponse.Errors);

            var signUpBody = new SignUpBody
            {
                Identifier = userInfo.Email,
                Method = DefaultAuthenticationMethod.Email,
                Nickname = userInfo.FirstName,
                Password = userInfo.FirstName,
                DeviceId = deviceId,
                DeviceTypeId = Enum.Parse<DeviceTypeId>(deviceTypeId)
            };

            var response = await _authService.SignUp(signUpBody, AccountRole.User.ToString(), AuthenticationProvider.MailRu);
            if (response.IsSuccess)
            {
                await _authService.CreateMailCredentials(userInfo.Email, tokenPairResponse.AccessToken, tokenPairResponse.RefreshToken, EmailProvider.MailRu);
                return StatusCode((int)response.StatusCode, response.Body);
            }
            return StatusCode((int)response.StatusCode, response.Errors);
        }

        private DeviceTypeId? GetDeviceType(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return null;

            var regex = new Regex("^[0-9a-fA-F]{16}$");
            if (regex.IsMatch(deviceId))
                return DeviceTypeId.AndroidId;
            if (Guid.TryParse(deviceId, out _))
                return DeviceTypeId.UUID;

            return null;
        }
    }
}