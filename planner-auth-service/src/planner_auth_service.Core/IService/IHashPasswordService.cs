namespace planner_auth_service.Core.IService
{
    public interface IHashPasswordService
    {
        string HashPassword(string password);
    }
}