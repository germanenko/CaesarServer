namespace Planner_chat_server.Core
{
    public static class Constants
    {
        public static readonly string FileServerUrl = Environment.GetEnvironmentVariable("FILE_SERVER_URL") ?? throw new Exception("FILE_SERVER_URL is not found in enviroment variables");
        public static readonly string WebUrlToChatAttachment = $"{FileServerUrl}/api/chat/download";
        public static readonly string WebUrlToChatIcon = $"{FileServerUrl}/api/chatIcon/download";
    }
}