using planner_server_package.Entities;
using System;

namespace planner_server_package.Events
{
    public class CreateBoardEvent
    {
        public BoardBody Board { get; set; }
        public Guid CreatorId { get; set; }
    }
}