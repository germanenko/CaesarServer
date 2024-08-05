using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planer_mailbox_service.Core.Entities.Request;
using Planer_mailbox_service.Core.Entities.Response;
using Planer_mailbox_service.Core.Enums;
using Planer_mailbox_service.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Planer_mailbox_service.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class MailboxController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IMailCredentialsService _mailCredentialsService;
        private readonly IMailboxService _mailboxService;
        private readonly ITokenService _tokenService;

        public MailboxController(
            IJwtService jwtService,
            IMailCredentialsService mailCredentialsService,
            IMailboxService mailboxService,
            ITokenService tokenService)
        {
            _jwtService = jwtService;
            _mailCredentialsService = mailCredentialsService;
            _mailboxService = mailboxService;
            _tokenService = tokenService;
        }

        [HttpGet("emails"), Authorize]
        [SwaggerOperation("Получить список сообщений с почты")]
        [SwaggerResponse(200, "Успешно", Type = typeof(List<EmailMessageInfoDto>))]
        [SwaggerResponse(400, "Некорректные данные")]
        [SwaggerResponse(401, "Неавторизованный доступ")]
        [SwaggerResponse(404, "Почтовый аккаунт не подключен")]
        public async Task<IActionResult> GetEmails(
            [FromHeader(Name = "Authorization")] string token,
            [FromQuery] int offset = 0,
            [FromQuery] int count = 10,
            [FromQuery] EmailProvider emailProvider = EmailProvider.Gmail)
        {
            var accountId = _jwtService.GetTokenPayload(token).AccountId;
            var response = await _mailCredentialsService.GetMailCredentials(accountId);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            var emailCredentials = response.Body?.FirstOrDefault(e => e.EmailProvider == emailProvider.ToString());
            if (emailCredentials == null)
                return NotFound("Mail credentials not found");

            var tokenSource = await _tokenService.GetUpdatedTokens(emailCredentials, emailProvider);
            if (tokenSource == null)
                return BadRequest("Failed to update tokens");

            string accessToken = tokenSource.Value.AccessToken ?? emailCredentials.AccessToken;
            var result = await _mailboxService.GetMessages(emailCredentials.Email, accessToken, offset, count, emailProvider);
            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Errors);

            return StatusCode((int)result.StatusCode, result.Body);
        }

        [HttpDelete("emails"), Authorize]
        [SwaggerOperation("Удалить письма по индексам")]
        [SwaggerResponse(204, "Успешно")]
        [SwaggerResponse(400, "Некорректные данные")]
        [SwaggerResponse(401, "Неавторизованный доступ")]
        [SwaggerResponse(404, "Почтовый аккаунт не подключен")]

        public async Task<IActionResult> RemoveEmails(
            [FromHeader(Name = "Authorization")] string token,
            [FromQuery, Required] int messageIndex,
            [FromQuery] EmailProvider emailProvider = EmailProvider.Gmail)
        {
            var accountId = _jwtService.GetTokenPayload(token).AccountId;
            var response = await _mailCredentialsService.GetMailCredentials(accountId);

            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            var emailCredentials = response.Body?.FirstOrDefault(e => e.EmailProvider == emailProvider.ToString());
            if (emailCredentials == null)
                return NotFound("Mail credentials not found");

            var tokenSource = await _tokenService.GetUpdatedTokens(emailCredentials, emailProvider);
            if (tokenSource == null)
                return BadRequest("Failed to update tokens");

            string accessToken = tokenSource.Value.AccessToken ?? emailCredentials.AccessToken;
            var result = await _mailboxService.DeleteMessages(emailCredentials.Email, accessToken, messageIndex, emailProvider);
            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Errors);

            return StatusCode((int)result.StatusCode);
        }

        [HttpPost("email"), Authorize]
        [SwaggerOperation("Отправить сообщение на почту")]
        [SwaggerResponse(200, "Успешно")]
        [SwaggerResponse(400, "Некорректные данные")]
        [SwaggerResponse(401, "Неавторизованный доступ")]
        [SwaggerResponse(404, "Нет подключенных ящиков")]
        [SwaggerResponse(415, "Неподдерживаемый формат почты")]
        public async Task<IActionResult> SendMessage(
            [FromHeader(Name = "Authorization")] string token,
            [FromBody] CreateMailMessageDto body)
        {
            var accountId = _jwtService.GetTokenPayload(token).AccountId;
            var response = await _mailCredentialsService.GetMailCredentials(accountId);
            if (!response.IsSuccess)
                return StatusCode((int)response.StatusCode, response.Errors);

            var emailCredentials = response.Body?.FirstOrDefault();
            if (emailCredentials == null)
                return NotFound("Mail credentials not found");

            var emailProvider = Enum.Parse<EmailProvider>(emailCredentials.EmailProvider);
            var tokenSource = await _tokenService.GetUpdatedTokens(emailCredentials, emailProvider);
            if (tokenSource == null)
                return BadRequest("Failed to update tokens");

            string password = emailCredentials.AccessToken;
            var result = await _mailboxService.SendMessage(emailCredentials.Email, "", body.ToEmail, "", body.Subject, body.Message, password, emailProvider);
            if (!result.IsSuccess)
                return StatusCode((int)result.StatusCode, result.Errors);

            return StatusCode((int)result.StatusCode);
        }
    }
}