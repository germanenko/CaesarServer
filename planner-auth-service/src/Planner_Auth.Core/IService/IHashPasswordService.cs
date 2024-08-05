namespace Planner_Auth.Core.IService
{
    public interface IHashPasswordService
    {
        string HashPassword(string password);
    }
}