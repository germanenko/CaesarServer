using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_common_package.Enums
{
    public enum OperationName
    {
        // WebSockets
        NotifyWebSocket,
        ChatWebSocket,

        // Chat
        GetChats,
        GetChat,
        GetChatMessages,
        GetAllChatMessages,
        CreatePersonalChat,
        SendMessageToEmail,
        GetChatsSettings,
        CreateOrUpdateMessageDraft,
        CreateOrUpdateMessageDrafts,
        SetEnabledNotifications,

        // Task
        GetTasks,
        GetAllTasks,
        CreateTask,
        CreateJobFromMessage,
        CreateTasks,
        UpdateTask,
        UpdateTasks,
        AddTaskToColumn,
        RemoveTaskFromColumn,

        // Board
        CreateOrUpdateBoard,
        CreateBoards,
        GetAllBoards,
        AddUserToBoard,
        GetBoardMembers,
        CreateColumn,
        CreateColumns,
        GetColumns,
        GetAllColumns,
        GetDefaultColumns,

        // User
        GetProfileIcon,
        GetProfile,
        UploadUserIcon,
        GetUsersByIdentifierPattern,
        GetOtherUserProfile,
        ChangePassword,
        AddFirebaseToken,
        UploadAvatar,

        // Authorization
        SignIn,
        SignUp,
        SendGoogleToken,
        GoogleAuth,
        RestoreToken,
        ValidateToken,

        // Content
        CreateOrUpdateNode,
        GetManifest,
        GetScopeManifest,
        GetNodes,
        GetNodeBranchesByRootIds,
        GetNodesByIds,
        SendAllUpdates,
        GetNodesByTypes,

        // Structure
        GetNodeLinks,
        ChangeNodeParent,
        ChangeNodePosition,
        CreateOrUpdateNodeLink,
        CreateOrUpdateNodeLinks,
        DeleteNode,

        // AccessRights
        GetAccessRules,
        GetCommonAccessRules,

        // Analytics
        SendAnalyticsAction,
        SendAnalyticsActions,

        // Design
        DownloadTheme,
        DownloadAllThemes
    }
}
