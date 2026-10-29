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
        GetChat,
        EditMessage,
        DeleteMessageForMe,
        DeleteMessage,
        ReadAttachedMessages,
        CreatePersonalChat,
        CreateOrUpdateMessageDraft,
        CreateOrUpdateMessageDrafts,
        SetEnabledNotifications,
        AttachMessageToJob,
        SendToMail,

        // Task
        CreateOrUpdateTask,
        CreateJobFromMessage,

        // Board
        CreateOrUpdateBoard,
        AddUserToBoard,
        CreateOrUpdateColumn,
        GetDefaultColumns,

        // User
        GetProfileIcon,
        GetProfile,
        UploadUserIcon,
        GetUsersByIdentifierPattern,
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
        GetManifest,
        GetScopeManifest,
        GetNodeBranchesByRootIds,
        GetNodesByIds,
        SendLocalUpdates,
        GetNodesByTypes,

        // Structure
        AddDefaultAttachColumn,
        ChangeNodeParent,
        ChangeNodePosition,
        DeleteNode,

        // AccessRights
        GrantAccess,
        ChangePermission,
        RevokeAccess,
        CreateOrUpdateAccessGroup,
        AddUserToGroup,
        RemoveUserFromGroup,
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
