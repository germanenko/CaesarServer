using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_notify_service.Core.IService
{
    public interface IFirebaseService
    {
        Task<string> SendNotificationAsync(string token, string title, string body, Dictionary<string, string>? data = null);
        Task<string> SendDataMessageAsync(string token, Dictionary<string, string> data);
        Task<BatchResponse> SendMulticastNotificationAsync(List<string> tokens, string title, string body, Dictionary<string, string>? data = null);
    }
}
