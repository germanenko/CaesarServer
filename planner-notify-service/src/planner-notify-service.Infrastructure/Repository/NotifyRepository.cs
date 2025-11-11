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

        public async Task<FirebaseToken?> AddFirebaseToken(Guid accountId, string firebaseToken)
        {
            var existingToken = await _context.FirebaseTokens
                .FirstOrDefaultAsync(x => x.UserId == accountId && x.Token == firebaseToken);

            if (existingToken == null)
            {
                var newToken = new FirebaseToken()
                {
                    UserId = accountId,
                    Token = firebaseToken
                };

                var token = _context.FirebaseTokens.Add(newToken).Entity;
                await _context.SaveChangesAsync();
                return token;
            }

            return existingToken;
        }
    }
}
