namespace planner_file_service.Core
{
    public static class Constants
    {
        public static readonly string LocalPathToStorages = Environment.GetEnvironmentVariable("FILE_SERVER_STORAGE_PATH") ?? throw new Exception("FILE_SERVER_STORAGE_PATH environment variable is not set");
    }
}