using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.Request
{
    public class CreateOrUpdateBoardBody
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Props { get; set; }
    }
}
