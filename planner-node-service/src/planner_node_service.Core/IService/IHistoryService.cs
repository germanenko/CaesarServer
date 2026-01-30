using planner_client_package.Entities;
using planner_common_package.Enums;
using planner_node_service.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace planner_node_service.Core.IService
{
    public interface IHistoryService
    {
        public Task<ServiceResponse<History>> GetCreateHistory(Guid nodeId);
        public Task<ServiceResponse<History>> AddHistory(History history);
    }
}
