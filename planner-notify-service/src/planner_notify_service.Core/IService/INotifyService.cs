using CaesarServerLibrary.Entities;
using planner_notify_service.Core.Entities.Models;
using planner_notify_service.Core.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_notify_service.Core.IService
{
    public interface INotifyService
    {
        Task<ServiceResponse<FirebaseToken>> AddFirebaseToken(Guid accountId, string firebaseToken, Guid deviceId);
        Task<ServiceResponse<bool>> SendFCMNotification(Guid userId, string title, string content, Dictionary<string, string> data);
    }
}
