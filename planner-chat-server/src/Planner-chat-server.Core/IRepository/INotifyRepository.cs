using Planner_chat_server.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Planner_chat_server.Core.IRepository
{
    public interface INotifyRepository
    {
        Task<FirebaseToken?> AddFirebaseToken(Guid account, string firebaseToken);
        Task<List<FirebaseToken>> GetTokens(List<Guid> accounts);
    }
}
