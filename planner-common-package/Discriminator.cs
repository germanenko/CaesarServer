using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_common_package
{
    public static class Discriminator
    {
        public const string TypeDiscriminatorPropertyName = "$type";
        public const string Board = "board";
        public const string Column = "column";
        public const string Task = "task";
        public const string Chat = "chat";
        public const string ChatMessage = "chatMessage";
        public const string NotificationSettings = "notificationSettings";
        public const string Profile = "profile";
    }
}
