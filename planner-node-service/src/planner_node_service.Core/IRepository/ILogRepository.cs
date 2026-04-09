using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.IRepository
{
    public interface ILogRepository
    {
        Task<History?> GetLastHistory(Guid nodeId);
        Task<ContentLog?> GetLastLogForScope(Guid scopeId);
    }
}
