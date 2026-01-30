using planner_common_package.Entities;
using planner_server_package.Interface;
using System.Collections.Generic;

namespace planner_server_package.Events
{
    public class SyncEntitiesEvent
    {
        public TokenPayload TokenPayload { get; set; }
        public List<ISyncable> Bodies { get; set; }
    }
}