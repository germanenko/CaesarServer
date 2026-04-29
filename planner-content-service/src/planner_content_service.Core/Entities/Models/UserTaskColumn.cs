using planner_client_package.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_content_service.Core.Entities.Models
{
    public class UserTaskColumn
    {
        public UserTaskColumn(Guid accountId, Guid columnId, Guid? chatId = null, Guid? id = null)
        {
            Id = id ?? Guid.NewGuid();
            AccountId = accountId;
            ColumnId = columnId;
            ChatId = chatId;
        }

        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public Guid ColumnId { get; set; }
        public Column Column { get; set; }
        public Guid? ChatId { get; set; }

        public TaskColumnBody ToBody()
        {
            return new TaskColumnBody()
            {
                Id = Id,
                ChatId = ChatId,
                ColumnId = ColumnId
            };
        }
    }
}
