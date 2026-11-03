using planner_client_package.Interface;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml;

namespace planner_client_package.Entities
{
    public class ChatBody : NodeBody
    {
        public string ImageUrl { get; set; }
        public int CountOfUnreadMessages { get; set; }
        public bool IsSyncedReadStatus { get; set; }
        public ChatType ChatType { get; set; }
        public List<Guid> ParticipantIds { get; set; } = new();
        public Guid? PartnerId { get; set; }
        public ChatStateBody State { get; set; }
        public ChatUserStateBody UserState { get; set; }
        public ChatEditBody ChatEdit { get; set; }
        public ChatSettingsBody ChatSettings { get; set; }

        public override IEnumerable<IBody> Extract()
        {
            var result = base.Extract().ToList();

            result.AddIfNotNull(ChatSettings);

            return result;
        }
    }
}