using planner_client_package.Entities.Request;
using planner_content_service.Core.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Core.IFactory
{
    public interface IJobFactory
    {
        Job Create(JobBodyRequest body);
    }
}
