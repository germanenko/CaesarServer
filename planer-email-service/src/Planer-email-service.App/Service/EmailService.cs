using System.Net;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using Planer_email_service.Core.Entities;
using Planer_email_service.Core.IService;

namespace Planer_email_service.App.Service
{
    public class EmailService : IEmailService
    {
        private readonly MailboxAddress _senderEmail;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderPassword;
        private readonly ILogger<IEmailService> _logger;

        public EmailService(
            string senderEmail,
            string senderPassword,
            string senderName,
            string smtpServer,
            int smtpPort,
            ILogger<IEmailService> logger
            )

        {
            _senderEmail = new MailboxAddress(senderName, senderEmail);
            _senderPassword = senderPassword;
            _smtpPort = smtpPort;
            _smtpServer = smtpServer;
            _logger = logger;
        }

        public async Task<ServiceResponse<string>> SendMessage(string email, string subject, string message)
        {
            return await SendMessageInternal(_senderEmail.Address, email, subject, message);
        }

        public async Task<ServiceResponse<string>> SendMessage(string fromEmail, string toEmail, string subject, string message)
        {
            return await SendMessageInternal(fromEmail, toEmail, subject, message);
        }

        private async Task<ServiceResponse<string>> SendMessageInternal(string fromEmail, string toEmail, string subject, string message)
        {
            try
            {
                using var emailMessage = new MimeMessage();
                emailMessage.Subject = subject;
                emailMessage.From.Add(new MailboxAddress("", fromEmail));
                emailMessage.To.Add(new MailboxAddress("", toEmail));
                emailMessage.Body = new TextPart()
                {
                    Text = message
                };

                using var client = new SmtpClient();
                client.CheckCertificateRevocation = false;
                await client.ConnectAsync(_smtpServer, _smtpPort, false);
                await client.AuthenticateAsync(_senderEmail.Address, _senderPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
                return new ServiceResponse<string>
                {
                    IsSuccess = true,
                    StatusCode = HttpStatusCode.OK,
                    Body = "Email sent successfully",
                    Errors = new string[] { }
                };
            }
            catch (SmtpCommandException ex)
            {
                return new ServiceResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = (HttpStatusCode)ex.StatusCode,
                    Errors = new string[] { "SMTP command error" }
                };
            }
            catch (SmtpProtocolException)
            {
                return new ServiceResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    Errors = new string[] { "SMTP protocol error" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("General error sending email: {Message}", ex.Message);
                return new ServiceResponse<string>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Errors = new string[] { "General error" }
                };
            }
        }
    }
}