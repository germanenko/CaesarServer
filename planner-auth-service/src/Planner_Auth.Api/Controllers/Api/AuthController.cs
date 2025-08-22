using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planner_Auth.App.Service;
using Planner_Auth.Core.Entities.Request;
using Planner_Auth.Core.Entities.Response;
using Planner_Auth.Core.Enums;
using Planner_Auth.Core.IService;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace Planner_Auth.Api.Controllers.Api
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
        public async Task<IActionResult> AddGoogleTokenAsync(string googleToken,
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
    }
}