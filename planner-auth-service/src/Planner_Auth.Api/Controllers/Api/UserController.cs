using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planner_Auth.Core.Entities.Response;
using Planner_Auth.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Planner_Auth.Api.Controllers.Api
{
    [ApiController]
    [Route("api")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public UserController(
            IUserService userService,
            IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpGet("profile"), Authorize]
        [SwaggerOperation("Получить профиль")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(ProfileBody))]
        public async Task<IActionResult> GetProfileAsync(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var response = await _userService.GetProfile(tokenInfo.AccountId);

            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode);
        }

        [HttpPatch("userTag"), Authorize]
        [SwaggerOperation("Изменить пользовательский тег")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> ChangeUserTag(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] string userTag
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var response = await _userService.ChangeAccountTag(tokenInfo.AccountId, userTag);
            return StatusCode((int)response);
        }

        [HttpGet("userTag")]
        [SwaggerOperation("Получить профиль пользователя по пользовательскому тегу")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetUserProfileByUserTag([FromQuery, Required] string userTag)
        {
            var response = await _userService.GetProfileByTag(userTag);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }

        [HttpGet("users/userTag")]
        [SwaggerOperation("Получить список пользователей по паттерну userTag")]
        [SwaggerResponse(200, Type = typeof(List<ProfileBody>))]

        public async Task<IActionResult> GetUsersByPatternUserTag([FromQuery, Required] string patternUserTag)
        {
            var response = await _userService.GetAllProfilesByPatternTag(patternUserTag);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }

        [HttpGet("users")]
        [SwaggerOperation("Получить список пользователей")]
        [SwaggerResponse(200, Type = typeof(List<ProfileBody>))]

        public async Task<IActionResult> GetUsers([FromQuery, Required] List<Guid> accountIds)
        {
            var response = await _userService.GetAllProfiles(accountIds);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }


        [HttpGet("user")]
        [SwaggerOperation("Получить профиль пользователя")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetUserInfo(
            [FromQuery, Required] string identifier
        )
        {
            var response = await _userService.GetProfile(identifier);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }


        [HttpGet("user/{id}")]
        [SwaggerOperation("Получить профиль пользователя по id")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetUserInfo(
            [FromRoute, Required] Guid id
        )
        {
            var response = await _userService.GetProfile(id);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }


        [HttpGet("users/identifier")]
        [SwaggerOperation("Получить список пользователей по паттерну")]
        [SwaggerResponse(200, Type = typeof(List<ProfileBody>))]

        public async Task<IActionResult> GetUsersBy(
            [FromQuery, Required] string identifierPattern
        )
        {
            var response = await _userService.GetAllProfilesByPatternIdentifier(identifierPattern);
            if (response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Body);

            return StatusCode((int)response.StatusCode, response.Errors);
        }
    }
}