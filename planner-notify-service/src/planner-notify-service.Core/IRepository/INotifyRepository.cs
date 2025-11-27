using planner_notify_service.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace planner_notify_service.Core.IRepository
{
    public interface INotifyRepository
    {
        Task<FirebaseToken?> AddFirebaseToken(Guid account, string firebaseToken, Guid deviceId);
        Task<List<FirebaseToken>?> GetTokens(Guid accountId);
        Task DeleteInvalidTokens();
    }
}
