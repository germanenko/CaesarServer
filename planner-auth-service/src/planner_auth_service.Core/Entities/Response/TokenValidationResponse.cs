using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_auth_service.Core.Entities.Response
{
    public record TokenValidationResponse
    {
        public bool Valid { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public TimeSpan TimeRemaining { get; set; }
    }
}
