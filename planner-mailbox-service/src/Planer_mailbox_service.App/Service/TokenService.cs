using Planer_mailbox_service.Core.Entities.Models;
using Planer_mailbox_service.Core.Enums;
using Planer_mailbox_service.Core.IService;

namespace Planer_mailbox_service.App.Service
{
    public class TokenService : ITokenService
    {
        private readonly IGoogleTokenService _googleTokenService;
        private readonly IMailRuTokenService _mailRuTokenService;

        public TokenService(
            IGoogleTokenService googleTokenService,
            IMailRuTokenService mailRuTokenService)
        {
            _googleTokenService = googleTokenService;
            _mailRuTokenService = mailRuTokenService;
        }

        public async Task<(string AccessToken, string RefreshToken)?> GetUpdatedTokens(AccountMailCredentials emailCredentials, EmailProvider emailProvider)
        {
            switch (emailProvider)
            {
                case EmailProvider.Gmail:
                    var googleResponse = await _googleTokenService.RefreshAccessTokenAsync(emailCredentials.RefreshToken);
                    if (googleResponse == null)
                        return null;

                    return (googleResponse.AccessToken, googleResponse.RefreshToken);

                case EmailProvider.MailRu:
                    var mailResponse = await _mailRuTokenService.UpdateToken(emailCredentials.RefreshToken);
                    if (mailResponse == null)
                        return null;

                    return (mailResponse.AccessToken, emailCredentials.RefreshToken);

                default:
                    return null;
            }
        }
    }
}