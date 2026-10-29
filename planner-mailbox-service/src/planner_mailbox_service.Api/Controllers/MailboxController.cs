using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using planner_common_package.Entities;
using planner_common_package.Enums;
using planner_mailbox_service.Api.CustomAttributes;
using planner_mailbox_service.Core.Entities.Request;
using planner_mailbox_service.Core.Entities.Response;
using planner_mailbox_service.Core.IService;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace planner_mailbox_service.Api.Controllers
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
                return StatusCode((int)response.StatusCode, new Response<List<EmailMessageInfoDto>>() { PrimaryErrorCode = response.PrimaryErrorCode, ErrorCodes = response.ErrorCodes });

            var emailCredentials = response.Body?.FirstOrDefault(e => e.EmailProvider == emailProvider.ToString());
            if (emailCredentials == null)
                return StatusCode((int)HttpStatusCode.NotFound, new Response<List<EmailMessageInfoDto>>()
                {
                    PrimaryErrorCode = ErrorCode.NotFound,
                    ErrorCodes = new List<ErrorCode> { ErrorCode.NotFound },
                    Reason = "Mail credentials not found"
                });

            var tokenSource = await _tokenService.GetUpdatedTokens(emailCredentials, emailProvider);
            if (tokenSource == null)
                return StatusCode((int)HttpStatusCode.BadRequest, new Response<List<EmailMessageInfoDto>>()
                {
                    PrimaryErrorCode = ErrorCode.ValidationError,
                    ErrorCodes = new List<ErrorCode> { ErrorCode.ValidationError },
                    Reason = "Failed to update tokens"
                });

            string accessToken = tokenSource.Value.AccessToken ?? emailCredentials.AccessToken;
            var result = await _mailboxService.GetMessages(emailCredentials.Email, accessToken, offset, count, emailProvider);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<List<EmailMessageInfoDto>>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<List<EmailMessageInfoDto>>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
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
                return StatusCode((int)response.StatusCode, new Response<string>() { PrimaryErrorCode = response.PrimaryErrorCode, ErrorCodes = response.ErrorCodes });

            var emailCredentials = response.Body?.FirstOrDefault(e => e.EmailProvider == emailProvider.ToString());
            if (emailCredentials == null)
                return StatusCode((int)HttpStatusCode.NotFound, new Response<string>() { PrimaryErrorCode = ErrorCode.NotFound, ErrorCodes = [ErrorCode.NotFound], Reason = "Mail credentials not found" });

            var tokenSource = await _tokenService.GetUpdatedTokens(emailCredentials, emailProvider);
            if (tokenSource == null)
                return StatusCode((int)HttpStatusCode.BadRequest, new Response<string>() { PrimaryErrorCode = ErrorCode.ValidationError, ErrorCodes = [ErrorCode.ValidationError], Reason = "Failed to update tokens" });

            string accessToken = tokenSource.Value.AccessToken ?? emailCredentials.AccessToken;
            var result = await _mailboxService.DeleteMessages(emailCredentials.Email, accessToken, messageIndex, emailProvider);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<string>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<string>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
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
                return StatusCode((int)response.StatusCode, new Response<string>() { PrimaryErrorCode = response.PrimaryErrorCode, ErrorCodes = response.ErrorCodes });

            var emailCredentials = response.Body?.FirstOrDefault();
            if (emailCredentials == null)
                return StatusCode((int)HttpStatusCode.NotFound, new Response<string>() { PrimaryErrorCode = ErrorCode.NotFound, ErrorCodes = [ErrorCode.NotFound], Reason = "Mail credentials not found" });

            var emailProvider = Enum.Parse<EmailProvider>(emailCredentials.EmailProvider);
            var tokenSource = await _tokenService.GetUpdatedTokens(emailCredentials, emailProvider);
            if (tokenSource == null)
                return StatusCode((int)HttpStatusCode.BadRequest, new Response<string>() { PrimaryErrorCode = ErrorCode.ValidationError, ErrorCodes = [ErrorCode.ValidationError], Reason = "Failed to update tokens" });

            string password = emailCredentials.AccessToken;
            var result = await _mailboxService.SendMessage(emailCredentials.Email, "", body.ToEmail, "", body.Subject, body.Message, password, emailProvider);

            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode, new Response<string>() { Body = result.Body });

            return StatusCode((int)result.StatusCode, new Response<string>() { PrimaryErrorCode = result.PrimaryErrorCode, ErrorCodes = result.ErrorCodes });
        }
    }
}