using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planner_chat_server.Core.IService
{
    public interface IUserService
    {
        Task<string> GetUserName(Guid userId);
    }
}
