using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using planner_auth_service.Core.Entities.Response;
using planner_auth_service.Core.IService;
using planner_common_package.Enums;
using planner_server_package.Entities;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace planner_auth_service.Api.Controllers.Api
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;

        public AuthController(IAuthService authService, IJwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }


        [SwaggerOperation("Регистрация")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(OutputAccountCredentialsBody))]
        [SwaggerResponse(400, "Токен не валиден или активирован")]
        [SwaggerResponse(409, "Почта уже существует")]

        [HttpPost("signup")]
        public async Task<IActionResult> SignUpAsync(SignUpBody signUpBody)
        {
            string role = Enum.GetName(AccountRole.User)!;
            var result = await _authService.SignUp(signUpBody, role, AuthenticationProvider.Default);
            if (result.IsSuccess)
                return Ok(result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [SwaggerOperation("Авторизация")]
        [SwaggerResponse(200, "Успешно", Type = typeof(OutputAccountCredentialsBody))]
        [SwaggerResponse(400, "Пароли не совпадают")]
        [SwaggerResponse(404, "Email не зарегистрирован")]
        [SwaggerResponse(409, "Аккаунт создан другим провайдером")]

        [HttpPost("signin")]
        public async Task<IActionResult> SignInAsync(SignInBody signInBody)
        {
            var result = await _authService.SignIn(signInBody, AuthenticationProvider.Default);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [SwaggerOperation("Добавить Google токен"), Authorize]
        [SwaggerResponse(200, "Успешно добавлен", Type = typeof(OutputAccountCredentialsBody))]
        [SwaggerResponse(400, "Токен не валиден или активирован")]

        [HttpPost("addGoogleToken")]
        public async Task<IActionResult> AddGoogleTokenAsync(GoogleTokenBody googleToken,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);

            var result = await _authService.AddGoogleToken(googleToken, tokenInfo.AccountId);
            if (result.IsSuccess == true)
                return Ok(result);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [SwaggerOperation("Получить Google токен"), Authorize]
        [SwaggerResponse(200, "Успешно получен", Type = typeof(OutputAccountCredentialsBody))]

        [HttpGet("getGoogleToken")]
        public async Task<IActionResult> GetGoogleTokenAsync(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);

            var result = await _authService.GetGoogleToken(tokenInfo.AccountId);
            if (result.IsSuccess)
                return Ok(result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [SwaggerOperation("Восстановление токена")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(OutputAccountCredentialsBody))]
        [SwaggerResponse(400, "Идентификатор устройства не валиден")]
        [SwaggerResponse(404, "Токен не используется")]

        [HttpPost("token")]
        public async Task<IActionResult> RestoreTokenAsync(
            TokenBody body,
            [FromHeader(Name = "DeviceId")] string deviceId,
            [FromHeader(Name = "DeviceTypeId")] DeviceTypeId deviceTypeId
        )
        {
            var result = await _authService.RestoreToken(body.Value, deviceId, deviceTypeId);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [SwaggerOperation("Google аутентификация")]
        [SwaggerResponse(200, "Успешно авторизован", Type = typeof(OutputAccountCredentialsBody))]
        [HttpPost("googleAuth")]
        public async Task<IActionResult> GoogleAuthAsync(GoogleTokenBody googleToken,
            [FromHeader(Name = "DeviceId")] string deviceId,
            [FromHeader(Name = "DeviceTypeId")] DeviceTypeId deviceTypeId)
        {
            var result = await _authService.GoogleAuth(googleToken, deviceTypeId, deviceId);
            if (result.IsSuccess)
                return Ok(result.Body);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        [HttpPut("resetPassword")]
        [SwaggerOperation("Сброс пароля")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(string))]
        public async Task<IActionResult> ResetPassword(
            [FromHeader(Name = "X-Reset-Token")] string token,
            [FromBody] string newPassword
        )
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required");

            if (!_jwtService.ValidatePasswordResetToken(token))
                return BadRequest("Invalid or expired token");

            var tokenInfo = _jwtService.GetPasswordResetTokenPayload(token);
            if (tokenInfo == null)
                return BadRequest("Invalid token payload");

            if (tokenInfo.ExpiresAt < DateTime.UtcNow)
                return BadRequest("Token has expired");

            var response = await _authService.ResetPassword(tokenInfo.AccountId, newPassword);

            if (response.IsSuccess)
            {
                _jwtService.InvalidatePasswordResetToken(token);
                return StatusCode((int)response.StatusCode);
            }

            return StatusCode((int)response.StatusCode, response.Errors);
        }

        [HttpPut("changePassword"), Authorize]
        [SwaggerOperation("Смена пароля")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(bool))]
        public async Task<IActionResult> ChangePassword(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery] string oldPassword,
            [FromQuery] string newPassword
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var response = await _authService.ChangePassword(tokenInfo.AccountId, oldPassword, newPassword);

            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode);
        }

        [HttpGet("validateResetToken")]
        [SwaggerOperation("Проверка валидности токена сброса пароля")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(bool))]
        public IActionResult ValidateResetToken(
            [FromQuery] string token
        )
        {
            var isValid = _jwtService.ValidatePasswordResetToken(token);
            var payload = _jwtService.GetPasswordResetTokenPayload(token);

            return Ok(new
            {
                Valid = isValid,
                ExpiresAt = payload?.ExpiresAt,
                TimeRemaining = payload != null ? payload.ExpiresAt - DateTime.UtcNow : TimeSpan.Zero
            });
        }
    }
}