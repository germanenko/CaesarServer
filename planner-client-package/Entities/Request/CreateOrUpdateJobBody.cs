using planner_client_package.Interface;
using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities.Request
{
    public class CreateOrUpdateJobBody : IRequest
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Props { get; set; }

        public string Description { get; set; }

        public NodeLinkBody Link { get; set; }
    }
}
