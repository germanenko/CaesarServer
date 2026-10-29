using planner_client_package.Entities;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Node
{
    public interface INodeService
    {
        Task<ServiceResponse<NodeBody>> CreateOrUpdateNode(Guid accountId, Guid requestId, NodeBody nodeBody, OperationName operationName);
    }
}
