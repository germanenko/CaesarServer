using planner_auth_service.Core.IService;
using System.Security.Cryptography;
using System.Text;

namespace planner_auth_service.App.Service
{
    public class HashPasswordService : IHashPasswordService
    {
        private readonly string _key;

        public HashPasswordService(string key)
        {
            _key = key;
        }

        public string HashPassword(string password)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_key);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }
    }
}