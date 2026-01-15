using System.Net;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using Planer_mailbox_service.Core.Entities.Response;
using Planer_mailbox_service.Core.Enums;
using Planer_mailbox_service.Core.IService;

namespace Planer_mailbox_service.App.Service
{
    public class MailboxService : IMailboxService
    {
        private readonly ILogger<MailboxService> _logger;

        public MailboxService(ILogger<MailboxService> logger)
        {
            _logger = logger;
        }

        public async Task<ServiceResponse<string>> DeleteMessages(
            string email,
            string accessToken,
            int messageIndex,
            EmailProvider emailProvider)
        {
            string imapServer = "imap.gmail.com";
            int port = 993;

            if (emailProvider == EmailProvider.MailRu)
                imapServer = "imap.mail.ru";

            using var client = new ImapClient();
            try
            {
                var serverResponse = await ConnectToServer(client, email, accessToken, imapServer, port);
                if (!serverResponse.IsSuccess)
                    return serverResponse;

                await client.Inbox.OpenAsync(FolderAccess.ReadWrite);

                var message = await client.Inbox.GetMessageAsync(messageIndex);
                await client.Inbox.AddFlagsAsync(messageIndex, MessageFlags.Deleted, true);

                await client.Inbox.ExpungeAsync();

                return new ServiceResponse<string>
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"IMAP: Failed to delete messages: {ex.Message}");
                return new ServiceResponse<string>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                };
            }
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(true);
            }
        }

        public async Task<ServiceResponse<string>> SendMessage(
            string fromEmail,
            string senderName,
            string toEmail,
            string toName,
            string subject,
            string message,
            string password,
            EmailProvider emailProvider)
        {

            string smtpServer = "smtp.gmail.com";
            int port = 587;

            if (emailProvider == EmailProvider.MailRu)
            {
                smtpServer = "smtp.mail.ru";
                port = 465;
            }

            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(senderName, fromEmail));
                emailMessage.To.Add(new MailboxAddress(toName, toEmail));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("plain") { Text = message };

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, port, SecureSocketOptions.StartTls);
                if (!client.IsConnected)
                    return new ServiceResponse<string>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Errors = new string[] { "Failed to connect to SMTP server" }
                    };

                var oauth2 = new SaslMechanismOAuth2(fromEmail, password);
                await client.AuthenticateAsync(oauth2);
                if (!client.IsAuthenticated)
                    return new ServiceResponse<string>
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        IsSuccess = false,
                        Errors = new string[] { "Failed to authenticate" }
                    };

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                return new ServiceResponse<string>
                {
                    StatusCode = HttpStatusCode.OK,
                    IsSuccess = true
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to send message: {e.Message}");
                return new ServiceResponse<string>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                };
            }
        }

        private async Task<ServiceResponse<string>> ConnectToServer(ImapClient client, string email, string accessToken, string imapServer, int port)
        {
            await client.ConnectAsync(imapServer, port, SecureSocketOptions.SslOnConnect);
            if (!client.IsConnected)
                return new ServiceResponse<string>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Errors = new string[] { "IMAP: Failed to connect to server" }
                };

            var oauth2 = new SaslMechanismOAuth2(email, accessToken);
            await client.AuthenticateAsync(oauth2);
            if (!client.IsAuthenticated)
                return new ServiceResponse<string>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    IsSuccess = false,
                    Errors = new string[] { "IMAP: Failed to authenticate" }
                };

            return new ServiceResponse<string>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true
            };
        }

        public async Task<ServiceResponse<List<EmailMessageInfoDto>>> GetMessages(string email, string access_token, int offset, int count, EmailProvider emailProvider)
        {
            var messages = new List<EmailMessageInfoDto>();
            string imapServer = "imap.gmail.com";
            int port = 993;

            if (emailProvider == EmailProvider.MailRu)
                imapServer = "imap.mail.ru";

            using var client = new ImapClient();
            try
            {
                var serverResponse = await ConnectToServer(client, email, access_token, imapServer, port);
                if (!serverResponse.IsSuccess)
                    return new ServiceResponse<List<EmailMessageInfoDto>>
                    {
                        StatusCode = serverResponse.StatusCode,
                        IsSuccess = serverResponse.IsSuccess,
                        Errors = serverResponse.Errors
                    };

                await FetchMessages(client, messages, offset, count);
            }
            catch (Exception ex)
            {
                _logger.LogError($"IMAP: Failed to get messages: {ex.Message}");
                return new ServiceResponse<List<EmailMessageInfoDto>>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    IsSuccess = false,
                };
            }
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(true);
            }

            return new ServiceResponse<List<EmailMessageInfoDto>>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = messages
            };
        }

        private async Task FetchMessages(ImapClient client, List<EmailMessageInfoDto> messages, int offset, int count)
        {
            await client.Inbox.OpenAsync(FolderAccess.ReadOnly);
            int messageCount = client.Inbox.Count;
            int start = offset;
            int end = Math.Min(start + count - 1, messageCount - 1);

            var messageSummaries = await client.Inbox.FetchAsync(start, end, MessageSummaryItems.All);
            foreach (var summary in messageSummaries)
            {
                try
                {
                    var message = await client.Inbox.GetMessageAsync(summary.Index);
                    messages.Add(new EmailMessageInfoDto
                    (
                        message.Subject,
                        message.From.Mailboxes.Select(x => x.Address).FirstOrDefault(),
                        message.To.Mailboxes.Select(x => x.Address).FirstOrDefault(),
                        message.Date.DateTime,
                        message.TextBody ?? message.HtmlBody,
                        summary.Index
                    ));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to fetch message with UID {summary.Index}: {ex.Message}");
                }
            }
        }

    }
}