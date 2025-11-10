using Microsoft.EntityFrameworkCore;
using Planner_chat_server.Core.Entities.Models;
using Planner_chat_server.Core.IRepository;
using Planner_chat_server.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Planner_chat_server.Infrastructure.Repository
{
    public class NotifyRepository : INotifyRepository
    {
        private readonly NotifyDbContext _context;

        public NotifyRepository(
            NotifyDbContext context)
        {
            _context = context;
        }

        public async Task<FirebaseToken?> AddFirebaseToken(Guid account, string firebaseToken)
        {
            var newToken = new FirebaseToken()
            {
                UserId = account,
                Token = firebaseToken
            };

            var token = _context.FirebaseTokens.Add(newToken).Entity;

            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<List<FirebaseToken>> GetTokens(List<Guid> accounts)
        {
            return await _context.FirebaseTokens
                .Where(t => accounts.Contains(t.UserId)).ToListAsync();
        }
    }
}
