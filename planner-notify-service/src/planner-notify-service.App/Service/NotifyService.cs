using planner_notify_service.Core.Entities.Models;
using planner_notify_service.Core.Entities.Response;
using planner_notify_service.Core.IRepository;
using planner_notify_service.Core.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace planner_notify_service.App.Service
{
    public class NotifyService : INotifyService
    {
        private readonly INotifyRepository _notifyRepository;
        private readonly IFirebaseService _firebaseService;

        public NotifyService(
            INotifyRepository notifyRepository, IFirebaseService firebaseService)
        {
            _notifyRepository = notifyRepository;
            _firebaseService = firebaseService;
        }

        public async Task<ServiceResponse<FirebaseToken>> AddFirebaseToken(Guid accountId, string firebaseToken, Guid deviceId)
        {
            var token = await _notifyRepository.AddFirebaseToken(accountId, firebaseToken, deviceId);

            return new ServiceResponse<FirebaseToken>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = token
            };
        }

        public async Task<ServiceResponse<bool>> SendFCMNotification(Guid userId, string title, string content)
        {
            var tokens = await _notifyRepository.GetTokens(userId);

            await _firebaseService.SendNotificationAsync(tokens, title, content);

            return new ServiceResponse<bool>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = true
            };
        }
    }
}
