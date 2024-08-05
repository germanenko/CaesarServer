using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Planer_mailbox_service.Core.Entities.Response
{
    public class TokenPayload
    {
        public Guid AccountId { get; set; }
        public string Role { get; set; }
        public Guid SessionId { get; set; }
    }
}