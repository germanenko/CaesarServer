using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner_chat_server.Core.IService
{
    public interface IFirebaseService
    {
        Task<bool> SendNotification(string firebaseToken, string title, string content);
        Task<string> SendNotificationAsync(string token, string title, string body, Dictionary<string, string>? data = null);
        Task<string> SendDataMessageAsync(string token, Dictionary<string, string> data);
        Task<BatchResponse> SendMulticastNotificationAsync(List<string> tokens, string title, string body, Dictionary<string, string>? data = null);
    }
}
