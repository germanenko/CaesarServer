using AccessGroupClient = planner_client_package.Entities.AccessGroupBody;
using AccessGroupServer = planner_server_package.Entities.AccessGroupBody;
using AccessRightClient = planner_client_package.Entities.AccessRightBody;
using AccessRightServer = planner_server_package.Entities.AccessRightBody;
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
using MessageClient = planner_client_package.Entities.MessageBody;
using MessageServer = planner_server_package.Entities.MessageBody;
using NodeLinkClient = planner_client_package.Entities.NodeLinkBody;
using NodeLinkServer = planner_server_package.Entities.NodeLinkBody;
using TaskClient = planner_client_package.Entities.TaskBody;
using TaskServer = planner_server_package.Entities.TaskBody;


namespace planner_server_package.Converters
{
    public static class BodyConverter
    {
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
                Link = body.Link != null ? ServerToClientBody(body.Link) : null,
                AccessRight = body.AccessRight != null ? ServerToClientBody(body.AccessRight) : null,
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
                Link = body.Link != null ? ClientToServerBody(body.Link) : null,
                AccessRight = body.AccessRight != null ? ClientToServerBody(body.AccessRight) : null
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
                Link = body.Link != null ? ServerToClientBody(body.Link) : null,
                AccessRight = body.AccessRight != null ? ServerToClientBody(body.AccessRight) : null
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
                Link = body.Link != null ? ClientToServerBody(body.Link) : null,
                AccessRight = body.AccessRight != null ? ClientToServerBody(body.AccessRight) : null
            };
        }


        public static TaskClient ServerToClientBody(TaskServer body)
        {
            return new TaskClient()
            {
                Id = body.Id,
                Name = body.Name,
                PublicationStatus = body.PublicationStatus,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                StartDate = body.StartDate,
                Status = body.Status,
                Description = body.Description,
                EndDate = body.EndDate,
                HexColor = body.HexColor,
                PriorityOrder = body.PriorityOrder,
                TaskType = body.TaskType,
                Link = body.Link != null ? ServerToClientBody(body.Link) : null,
                AccessRight = body.AccessRight != null ? ServerToClientBody(body.AccessRight) : null
            };
        }



        public static TaskServer ClientToServerBody(TaskClient body)
        {
            return new TaskServer()
            {
                Id = body.Id,
                Name = body.Name,
                PublicationStatus = body.PublicationStatus,
                Props = body.Props,
                Type = body.Type,
                UpdatedAt = body.UpdatedAt,
                UpdatedBy = body.UpdatedBy,
                Link = body.Link != null ? ClientToServerBody(body.Link) : null,
                AccessRight = body.AccessRight != null ? ClientToServerBody(body.AccessRight) : null,
                Status = body.Status,
                StartDate = body.StartDate,
                Description = body.Description,
                EndDate = body.EndDate,
                HexColor = body.HexColor,
                TaskType = body.TaskType,
                PriorityOrder = body.PriorityOrder
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
                ParticipantIds = body.ParticipantIds,
                ChatType = body.ChatType,
                ImageUrl = body.ImageUrl,
                CountOfUnreadMessages = body.CountOfUnreadMessages,
                IsSyncedReadStatus = body.IsSyncedReadStatus,
                Link = body.Link != null ? ServerToClientBody(body.Link) : null,
                AccessRight = body.AccessRight != null ? ServerToClientBody(body.AccessRight) : null
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
                IsSyncedReadStatus = body.IsSyncedReadStatus,
                ChatType = body.ChatType,
                ImageUrl = body.ImageUrl,
                CountOfUnreadMessages = body.CountOfUnreadMessages,
                ParticipantIds = body.ParticipantIds,
                Link = body.Link != null ? ClientToServerBody(body.Link) : null,
                AccessRight = body.AccessRight != null ? ClientToServerBody(body.AccessRight) : null
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
                SenderDeviceId = body.SenderDeviceId,
                SenderId = body.SenderId,
                ChatId = body.ChatId,
                Content = body.Content,
                Date = body.Date,
                HasBeenRead = body.HasBeenRead,
                MessageType = body.MessageType,
                Link = body.Link != null ? ServerToClientBody(body.Link) : null,
                AccessRight = body.AccessRight != null ? ServerToClientBody(body.AccessRight) : null
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
                SenderDeviceId = body.SenderDeviceId,
                SenderId = body.SenderId,
                ChatId = body.ChatId,
                Content = body.Content,
                Date = body.Date,
                HasBeenRead = body.HasBeenRead,
                MessageType = body.MessageType,
                Link = body.Link != null ? ClientToServerBody(body.Link) : null,
                AccessRight = body.AccessRight != null ? ClientToServerBody(body.AccessRight) : null
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


        public static AccessRightClient ServerToClientBody(AccessRightServer body)
        {
            return new AccessRightClient()
            {
                Id = body.Id,
                AccessGroupId = body.AccessGroupId,
                AccessType = body.AccessType,
                AccountId = body.AccountId,
                NodeId = body.NodeId,
                AccessGroup = body.AccessGroup != null ? ServerToClientBody(body.AccessGroup) : null
            };
        }



        public static AccessRightServer ClientToServerBody(AccessRightClient body)
        {
            return new AccessRightServer()
            {
                Id = body.Id,
                AccessGroupId = body.AccessGroupId,
                AccessType = body.AccessType,
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
