using planner_server_package.Enums;
using System;

namespace planner_server_package.Entities
{
    public class ProfileBody
    {
        public Guid Id { get; set; }
        public string Identifier { get; set; }
        public string Nickname { get; set; }
        public AccountRole Role { get; set; }
        public string UrlIcon { get; set; }
        public string UserTag { get; set; }
        public DefaultAuthenticationMethod IdentifierType { get; set; }
    }
}