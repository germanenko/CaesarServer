using Microsoft.EntityFrameworkCore;
using Planner_Auth.Core.Entities.Models;
using Planner_Auth.Core.Enums;
using Planner_Auth.Core.IRepository;
using Planner_Auth.Infrastructure.Data;

namespace Planner_Auth.Infrastructure.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AuthDbContext _context;

        public AccountRepository(
            AuthDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> AddAsync(string identifier, string nickname, string password, string deviceId, DeviceTypeId deviceTypeId, DefaultAuthenticationMethod authenticationMethod, string role, AuthenticationProvider providerType)
        {
            var account = await GetAsync(identifier);
            if (account != null)
                return null;

            account = new Account
            {
                Identifier = identifier,
                AuthenticationMethod = authenticationMethod.ToString(),
                Nickname = nickname,
                PasswordHash = password,
                RoleName = role,
                AuthorizationProvider = providerType.ToString(),
            };

            var result = await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
            return result?.Entity;
        }

        public async Task<AccountSession?> GetSessionAsync(Guid accountId, string deviceId)
        {
            return await _context.AccountSessions
                .FirstOrDefaultAsync(e => e.AccountId == accountId && e.DeviceId == deviceId);
        }

        public async Task<List<AccountSession>> GetSessionsAsync(Guid accountId)
        {
            return await _context.AccountSessions
                    .Where(e => e.AccountId == accountId)
                .ToListAsync();
        }

        public async Task<AccountSession?> AddSessionAsync(string deviceId, Account account)
        {
            var session = await GetSessionAsync(account.Id, deviceId);
            if (session != null)
                return null;

            session = new AccountSession
            {
                DeviceId = deviceId,
                Account = account,
            };

            var result = await _context.AccountSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            return result?.Entity;
        }


        public async Task<List<Account>> GetAccountsAsync(List<string> identifiers)
        {
            var result = await _context.Accounts.Where(e => identifiers.Contains(e.Identifier))
                .ToListAsync();
            return result;
        }

        public async Task<Account?> GetAsync(Guid id)
            => await _context.Accounts
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<Account?> GetAsync(string identifier)
        {
            return await _context.Accounts
                .FirstOrDefaultAsync(e => e.Identifier == identifier);
        }

        public async Task<AccountSession?> GetSessionByTokenAndAccount(string refreshTokenHash)
            => await _context.AccountSessions
            .Include(e => e.Account)
            .FirstOrDefaultAsync(e => e.Token == refreshTokenHash);

        public async Task<List<Account>> GetAccountsByPatternIdentifier(string identifier)
        {
            return await _context.Accounts
                .Where(e => EF.Functions.Like(e.Identifier, $"%{identifier}%"))
                .ToListAsync();
        }


        public async Task<Account?> UpdateProfileIconAsync(Guid accountId, string filename)
        {
            var account = await GetAsync(accountId);
            if (account == null)
                return null;

            account.Image = filename;
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<string?> UpdateTokenAsync(string refreshToken, Guid sessionId, TimeSpan? duration = null)
        {
            var session = await GetSessionAsync(sessionId);
            if (session == null)
                return null;

            if (duration == null)
                duration = TimeSpan.FromDays(15);

            if (session.TokenValidBefore <= DateTime.UtcNow || session.TokenValidBefore == null)
            {
                session.TokenValidBefore = DateTime.UtcNow.Add((TimeSpan)duration);
                session.Token = refreshToken;
                await _context.SaveChangesAsync();
            }

            return session.Token;
        }

        public async Task<Account?> UpdateAccountTagAsync(Guid id, string accountTag)
        {
            var account = await GetAsync(id);
            if (account == null)
                return null;

            var existedAccount = await GetAccountByTagAsync(accountTag);
            if (existedAccount != null)
                return null;

            account.Tag = accountTag;
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<Account?> GetAccountByTagAsync(string accountTag)
        {
            return await _context.Accounts.FirstOrDefaultAsync(e => e.Tag == accountTag);
        }

        public async Task<List<Account>> GetAccountsByPatternAccountTag(string patternAccountTag)
        {
            return await _context.Accounts
               .Where(e => e.Tag != null && EF.Functions.Like(e.Tag, $"%{patternAccountTag}%"))
               .ToListAsync();
        }

        public async Task<AccountSession?> GetSessionAsync(Guid sessionId) => await _context.AccountSessions.FirstOrDefaultAsync(e => e.Id == sessionId);

        public async Task<List<Account>> GetAccountsAsync(IEnumerable<Guid> ids)
        {
            return await _context.Accounts.Where(e => ids.Contains(e.Id))
                .ToListAsync();
        }
    }
}