using planner_client_package.Enums;
using System;
using System.Collections.Generic;

namespace planner_client_package.Entities
{
    public class ChatBody : NodeBody
    {
        public string ImageUrl { get; set; }
        public int CountOfUnreadMessages { get; set; }
        public MessageBody LastMessage { get; set; }
        public bool IsSyncedReadStatus { get; set; }
        public ChatType ChatType { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new();
        public ProfileBody Profile { get; set; }
    }
}