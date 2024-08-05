using System.ComponentModel.DataAnnotations;
using Planner_Auth.Core.Enums;

namespace Planner_Auth.Core.Entities.Request
{
    public class SignUpBody
    {
        [Required]
        public string Identifier { get; set; }

        [Required]
        public string Nickname { get; set; }

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