using planner_server_package.Events.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.RabbitMQ
{
    public interface IPublisherService
    {
        Task<ServiceResponse<object>> Publish<T>(T message, PublishEvent publishEvent);
    }
}
