using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner_chat_server.Core.IService
{
    public interface INotificationService
    {
        Task<bool> SendNotification(Guid userId, string title, string content, Dictionary<string, string>? data = null);
    }
}
