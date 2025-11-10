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

        public NotifyService(
            INotifyRepository notifyRepository)
        {
            _notifyRepository = notifyRepository;
        }

        public async Task<ServiceResponse<FirebaseToken>> AddFirebaseToken(Guid accountId, string firebaseToken)
        {
            var token = await _notifyRepository.AddFirebaseToken(accountId, firebaseToken);

            return new ServiceResponse<FirebaseToken>
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = true,
                Body = token
            };
        }
    }
}
