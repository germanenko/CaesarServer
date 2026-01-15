using System.Net;
using Microsoft.EntityFrameworkCore;
using Planer_mailbox_service.Core.Entities.Models;
using Planer_mailbox_service.Core.Entities.Response;
using Planer_mailbox_service.Core.Enums;
using Planer_mailbox_service.Core.IService;
using Planer_mailbox_service.Infrastructure.Data;

namespace Planer_mailbox_service.App.Service
{
    public class MailCredentialsService : IMailCredentialsService
    {
        private readonly AccountCredentialsDbContext _context;

        public MailCredentialsService(AccountCredentialsDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse<List<AccountMailCredentials>>> GetMailCredentials(Guid accountId)
        {
            var mailCredentials = await _context.MailCredentials.Where(e => e.AccountId == accountId).ToListAsync();

            return new ServiceResponse<List<AccountMailCredentials>>
            {
                Body = mailCredentials,
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ServiceResponse<AccountMailCredentials>> AddMailCredential(string email, string access_token, string refresh_token, Guid accountId, EmailProvider emailProvider)
        {
            var response = await GetMailCredential(email);
            if (response.Body != null)
                return new ServiceResponse<AccountMailCredentials>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.Conflict
                };

            var mailCredentials = new AccountMailCredentials
            {
                AccessToken = access_token,
                Email = email,
                EmailProvider = emailProvider.ToString(),
                RefreshToken = refresh_token,
                AccountId = accountId
            };

            mailCredentials = (await _context.MailCredentials.AddAsync(mailCredentials))?.Entity;
            await _context.SaveChangesAsync();

            return new ServiceResponse<AccountMailCredentials>
            {
                Body = mailCredentials,
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<ServiceResponse<AccountMailCredentials>> GetMailCredential(string email)
        {
            var mailCredentials = await _context.MailCredentials.FirstOrDefaultAsync(e => e.Email == email);
            if (mailCredentials == null)
                return new ServiceResponse<AccountMailCredentials>
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound
                };

            return new ServiceResponse<AccountMailCredentials>
            {
                Body = mailCredentials,
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}