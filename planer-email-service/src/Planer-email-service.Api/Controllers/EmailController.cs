using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Planer_email_service.Core.Entities;
using Planer_email_service.Core.IService;
using Swashbuckle.AspNetCore.Annotations;

namespace Planer_email_service.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly IAccountService _accountService;
        private readonly IJwtService _jwtService;

        public EmailController(
            IEmailService emailService,
            IAccountService accountService,
            IJwtService jwtService)
        {
            _emailService = emailService;
            _accountService = accountService;
            _jwtService = jwtService;
        }

        [SwaggerOperation("Отправить письмо на почту")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(401)]

        [HttpPost("api/message/email"), Authorize]
        public async Task<IActionResult> SendMessageByEmail(
            SentMessageBody message,
            [FromQuery, EmailAddress] string email,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var account = await _accountService.GetAccountAsync(tokenPayload.AccountId);
            if (account == null || !IsValidEMail(account.Identifier))
            {
                var response = await _emailService.SendMessage(email, message.Subject, message.Content);
                if (response.IsSuccess)
                    return StatusCode((int)response.StatusCode);

                return StatusCode((int)response.StatusCode, response.Errors);
            }

            var result = await _emailService.SendMessage(account.Identifier, email, message.Subject, message.Content);
            if (result.IsSuccess)
                return StatusCode((int)result.StatusCode);

            return StatusCode((int)result.StatusCode, result.Errors);
        }

        private bool IsValidEMail(string email) => Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
    }
}