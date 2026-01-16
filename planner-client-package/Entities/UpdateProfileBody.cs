using System;

namespace planner_client_package.Entities
{
    public class UpdateProfileBody
    {
        public Guid AccountId { get; set; }
        public string FileName { get; set; }
    }
}