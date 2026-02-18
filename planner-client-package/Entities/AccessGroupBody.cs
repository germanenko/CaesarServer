using planner_client_package.Interface;
using System;

namespace planner_client_package.Entities
{
    public class AccessGroupBody : IBody
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
