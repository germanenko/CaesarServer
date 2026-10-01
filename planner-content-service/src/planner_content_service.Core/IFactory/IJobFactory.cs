using planner_client_package.Entities;
using planner_client_package.Entities.Request;
using planner_content_service.Core.Entities.Models;

namespace planner_content_service.Core.IFactory
{
    public interface IJobFactory
    {
        Job Create(JobBodyRequest body);
        Job CreateFromBody(JobBody body);
    }
}
