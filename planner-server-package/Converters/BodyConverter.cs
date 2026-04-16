using AccessGroupClient = planner_client_package.Entities.AccessGroupBody;
using AccessGroupServer = planner_server_package.Entities.AccessGroupBody;
using AccessRuleClient = planner_client_package.Entities.AccessRuleBody;
using AccessRuleServer = planner_server_package.Entities.AccessRuleBody;
using AccountSessionsClient = planner_client_package.Entities.AccountSessions;
using AccountSessionsServer = planner_server_package.Entities.AccountSessions;
using BoardClient = planner_client_package.Entities.BoardBody;
using BoardServer = planner_server_package.Entities.BoardBody;
using ChatClient = planner_client_package.Entities.ChatBody;
using ChatServer = planner_server_package.Entities.ChatBody;
using ChatSettingsClient = planner_client_package.Entities.ChatSettingsBody;
using ChatSettingsServer = planner_server_package.Entities.ChatSettingsBody;
using ColumnClient = planner_client_package.Entities.ColumnBody;
using ColumnServer = planner_server_package.Entities.ColumnBody;
using JobClient = planner_client_package.Entities.JobBody;
using JobServer = planner_server_package.Entities.JobBody;
using MessageClient = planner_client_package.Entities.MessageBody;
using MessageServer = planner_server_package.Entities.MessageBody;
using NodeClient = planner_client_package.Entities.NodeBody;
using NodeLinkClient = planner_client_package.Entities.NodeLinkBody;
using NodeLinkServer = planner_server_package.Entities.NodeLinkBody;
using NodeServer = planner_server_package.Entities.NodeBody;


namespace planner_server_package.Converters
{
    public static class BodyConverter
    {
        public static NodeClient ServerToClientBody(NodeServer body)
        {
            return new NodeClient()
            {
                Id = body.Id,
                Name = body.Name,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                Link = body.Link != null ? ServerToClientBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ServerToClientBody(body.AccessRule) : null,
                Version = body.Version
            };
        }



        public static NodeServer ClientToServerBody(NodeClient body)
        {
            return new NodeServer()
            {
                Id = body.Id,
                Name = body.Name,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                Link = body.Link != null ? ClientToServerBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ClientToServerBody(body.AccessRule) : null,
                Version = body.Version
            };
        }


        public static BoardClient ServerToClientBody(BoardServer body)
        {
            return new BoardClient()
            {
                Id = body.Id,
                Name = body.Name,
                PublicationStatus = body.PublicationStatus,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                Link = body.Link != null ? ServerToClientBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ServerToClientBody(body.AccessRule) : null,
                Version = body.Version
            };
        }



        public static BoardServer ClientToServerBody(BoardClient body)
        {
            return new BoardServer()
            {
                Id = body.Id,
                Name = body.Name,
                PublicationStatus = body.PublicationStatus,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                Link = body.Link != null ? ClientToServerBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ClientToServerBody(body.AccessRule) : null,
                Version = body.Version
            };
        }


        public static ColumnClient ServerToClientBody(ColumnServer body)
        {
            return new ColumnClient()
            {
                Id = body.Id,
                Name = body.Name,
                PublicationStatus = body.PublicationStatus,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                Link = body.Link != null ? ServerToClientBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ServerToClientBody(body.AccessRule) : null,
                Version = body.Version
            };
        }



        public static ColumnServer ClientToServerBody(ColumnClient body)
        {
            return new ColumnServer()
            {
                Id = body.Id,
                Name = body.Name,
                PublicationStatus = body.PublicationStatus,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                Link = body.Link != null ? ClientToServerBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ClientToServerBody(body.AccessRule) : null,
                Version = body.Version
            };
        }


        public static JobClient ServerToClientBody(JobServer body)
        {
            return new JobClient()
            {
                Id = body.Id,
                Name = body.Name,
                PublicationStatus = body.PublicationStatus,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                StartDate = body.StartDate,
                Status = body.Status,
                Description = body.Description,
                EndDate = body.EndDate,
                HexColor = body.HexColor,
                PriorityOrder = body.PriorityOrder,
                JobType = body.JobType,
                Link = body.Link != null ? ServerToClientBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ServerToClientBody(body.AccessRule) : null,
                Version = body.Version
            };
        }



        public static JobServer ClientToServerBody(JobClient body)
        {
            return new JobServer()
            {
                Id = body.Id,
                Name = body.Name,
                PublicationStatus = body.PublicationStatus,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                Link = body.Link != null ? ClientToServerBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ClientToServerBody(body.AccessRule) : null,
                Status = body.Status,
                StartDate = body.StartDate,
                Description = body.Description,
                EndDate = body.EndDate,
                HexColor = body.HexColor,
                JobType = body.JobType,
                PriorityOrder = body.PriorityOrder,
                Version = body.Version
            };
        }


        public static ChatClient ServerToClientBody(ChatServer body)
        {
            return new ChatClient()
            {
                Id = body.Id,
                Name = body.Name,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                ParticipantIds = body.ParticipantIds,
                ChatType = body.ChatType,
                ImageUrl = body.ImageUrl,
                CountOfUnreadMessages = body.CountOfUnreadMessages,
                IsSyncedReadStatus = body.IsSyncedReadStatus,
                Link = body.Link != null ? ServerToClientBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ServerToClientBody(body.AccessRule) : null,
                Version = body.Version
            };
        }



        public static ChatServer ClientToServerBody(ChatClient body)
        {
            return new ChatServer()
            {
                Id = body.Id,
                Name = body.Name,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                IsSyncedReadStatus = body.IsSyncedReadStatus,
                ChatType = body.ChatType,
                ImageUrl = body.ImageUrl,
                CountOfUnreadMessages = body.CountOfUnreadMessages,
                ParticipantIds = body.ParticipantIds,
                Link = body.Link != null ? ClientToServerBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ClientToServerBody(body.AccessRule) : null,
                Version = body.Version
            };
        }


        public static MessageClient ServerToClientBody(MessageServer body)
        {
            return new MessageClient()
            {
                Id = body.Id,
                Name = body.Name,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                SenderDeviceId = body.SenderDeviceId,
                SenderId = body.SenderId,
                Content = body.Content,
                SentAt = body.SentAt,
                HasBeenRead = body.HasBeenRead,
                MessageType = body.MessageType,
                Link = body.Link != null ? ServerToClientBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ServerToClientBody(body.AccessRule) : null,
                Version = body.Version
            };
        }



        public static MessageServer ClientToServerBody(MessageClient body)
        {
            return new MessageServer()
            {
                Id = body.Id,
                Name = body.Name,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                SyncKind = body.SyncKind,
                SenderDeviceId = body.SenderDeviceId,
                SenderId = body.SenderId,
                Content = body.Content,
                SentAt = body.SentAt,
                HasBeenRead = body.HasBeenRead,
                MessageType = body.MessageType,
                Link = body.Link != null ? ClientToServerBody(body.Link) : null,
                AccessRule = body.AccessRule != null ? ClientToServerBody(body.AccessRule) : null,
                Version = body.Version
            };
        }


        public static ChatSettingsClient ServerToClientBody(ChatSettingsServer body)
        {
            return new ChatSettingsClient()
            {
                Id = body.Id,
                AccountId = body.AccountId,
                ChatId = body.ChatId,
                ChatName = body.ChatName,
                DateLastViewing = body.DateLastViewing,
                MessageDraft = body.MessageDraft
            };
        }



        public static ChatSettingsServer ClientToServerBody(ChatSettingsClient body)
        {
            return new ChatSettingsServer()
            {
                Id = body.Id,
                AccountId = body.AccountId,
                ChatId = body.ChatId,
                ChatName = body.ChatName,
                DateLastViewing = body.DateLastViewing,
                MessageDraft = body.MessageDraft
            };
        }


        public static AccountSessionsClient ServerToClientBody(AccountSessionsServer body)
        {
            return new AccountSessionsClient()
            {
                AccountId = body.AccountId,
                SessionIds = body.SessionIds
            };
        }



        public static AccountSessionsServer ClientToServerBody(AccountSessionsClient body)
        {
            return new AccountSessionsServer()
            {
                AccountId = body.AccountId,
                SessionIds = body.SessionIds
            };
        }


        public static NodeLinkClient ServerToClientBody(NodeLinkServer body)
        {
            return new NodeLinkClient()
            {
                Id = body.Id,
                ChildId = body.ChildId,
                ParentId = body.ParentId,
                RelationType = body.RelationType
            };
        }



        public static NodeLinkServer ClientToServerBody(NodeLinkClient body)
        {
            return new NodeLinkServer()
            {
                Id = body.Id,
                ChildId = body.ChildId,
                ParentId = body.ParentId,
                RelationType = body.RelationType,
            };
        }


        public static AccessRuleClient ServerToClientBody(AccessRuleServer body)
        {
            return new AccessRuleClient()
            {
                Id = body.Id,
                AccessGroupId = body.AccessGroupId,
                Permission = body.Permission,
                AccountId = body.AccountId,
                NodeId = body.NodeId,
                AccessGroup = body.AccessGroup != null ? ServerToClientBody(body.AccessGroup) : null
            };
        }



        public static AccessRuleServer ClientToServerBody(AccessRuleClient body)
        {
            return new AccessRuleServer()
            {
                Id = body.Id,
                AccessGroupId = body.AccessGroupId,
                Permission = body.Permission,
                AccountId = body.AccountId,
                NodeId = body.NodeId,
                AccessGroup = body.AccessGroup != null ? ClientToServerBody(body.AccessGroup) : null
            };
        }


        public static AccessGroupClient ServerToClientBody(AccessGroupServer body)
        {
            return new AccessGroupClient()
            {
                Id = body.Id,
                Name = body.Name
            };
        }



        public static AccessGroupServer ClientToServerBody(AccessGroupClient body)
        {
            return new AccessGroupServer()
            {
                Id = body.Id,
                Name = body.Name
            };
        }
    }
}
