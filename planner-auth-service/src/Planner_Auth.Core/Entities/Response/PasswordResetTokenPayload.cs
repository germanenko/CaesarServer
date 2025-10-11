using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner_Auth.Core.Entities.Response
{
    public class PasswordResetTokenPayload
    {
        public string Email { get; set; }
        public string UserId { get; set; }
        public string TokenId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired => ExpiresAt < DateTime.UtcNow;
    }
}
