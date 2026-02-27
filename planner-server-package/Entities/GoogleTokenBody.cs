using System;
using System.ComponentModel.DataAnnotations;

namespace planner_server_package.Entities
{
    public class GoogleTokenBody
    {
        public Guid AccountId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

    }
}
