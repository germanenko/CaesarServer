using Microsoft.EntityFrameworkCore;
using planner_notify_service.Core.Entities.Models;
using planner_notify_service.Core.IRepository;
using planner_notify_service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace planner_notify_service.Infrastructure.Repository
{
    public class NotifyRepository : INotifyRepository
    {
        private readonly NotifyDbContext _context;

        public NotifyRepository(
            NotifyDbContext context)
        {
            _context = context;
        }

        public async Task<FirebaseToken?> AddFirebaseToken(Guid accountId, string firebaseToken, Guid deviceId)
        {
            var existingToken = await _context.FirebaseTokens
                .FirstOrDefaultAsync(x => x.UserId == accountId && x.DeviceId == deviceId);

            if (existingToken == null)
            {
                var newToken = new FirebaseToken()
                {
                    UserId = accountId,
                    Token = firebaseToken,
                    DeviceId = deviceId,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var token = _context.FirebaseTokens.Add(newToken).Entity;
                await _context.SaveChangesAsync();
                return token;
            }

            if (existingToken.Token != firebaseToken)
            {
                _context.FirebaseTokens.Remove(existingToken);
                await _context.SaveChangesAsync();

                var newToken = new FirebaseToken()
                {
                    UserId = accountId,
                    Token = firebaseToken,
                    DeviceId = deviceId,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.FirebaseTokens.Add(newToken);
                await _context.SaveChangesAsync();
                return newToken;
            }

            return existingToken;
        }

        public async Task<List<FirebaseToken>?> GetTokens(Guid accountId)
        {
            var token = await _context.FirebaseTokens.Where(x => x.UserId == accountId).ToListAsync();

            return token;
        }

        public async Task DeleteInvalidTokens()
        {
            var thresholdDate = DateTime.UtcNow - TimeSpan.FromDays(30);

            var devicesToDelete = await _context.FirebaseTokens
                .Where(d => d.IsActive == false)
                .Where(d => d.UpdatedAt < thresholdDate)
                .ToListAsync();

            if (devicesToDelete.Any())
            {
                _context.FirebaseTokens.RemoveRange(devicesToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}
