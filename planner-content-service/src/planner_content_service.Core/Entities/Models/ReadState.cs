using planner_client_package.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Core.Entities.Models
{
    public class ReadState
    {
        public ReadState(Guid jobId, Guid accountId)
        {
            JobId = jobId;
            AccountId = accountId;
            LastReadAtUtc = DateTime.UtcNow;
        }

        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public Guid AccountId { get; set; }
        public DateTime LastReadAtUtc { get; set; }

        public ReadStateBody ToBody()
        {
            return new ReadStateBody
            {
                Id = Id,
                JobId = JobId,
                AccountId = AccountId,
                LastReadAtUtc = LastReadAtUtc
            };
        }
    }
}
