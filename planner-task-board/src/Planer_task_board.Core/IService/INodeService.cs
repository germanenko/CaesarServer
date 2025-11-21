using Planer_task_board.Core.Entities.Models;
using Planer_task_board.Core.Entities.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Planer_task_board.Core.IService
{
    public interface INodeService
    {
        public Task<ServiceResponse<IEnumerable<Node>>> GetNodes(Guid accountId);
        public Task<ServiceResponse<Node>> AddOrUpdateNode(Guid accountId, Node node);
    }
}
