using planner_common_package.Enums;
using System.ComponentModel.DataAnnotations;

namespace planner_server_package.Entities
{
    public class SignInBody
    {
        [Required]
        public string Identifier { get; set; }

        [Required]
        public string Password { get; set; }

        [EnumDataType(typeof(DefaultAuthenticationMethod))]
        public DefaultAuthenticationMethod Method { get; set; }

        [EnumDataType(typeof(DeviceTypeId))]
        public DeviceTypeId DeviceTypeId { get; set; }

        [Required]
        public string DeviceId { get; set; }
    }
}