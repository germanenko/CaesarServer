using CaesarServerLibrary.Entities;
using System;

namespace CaesarServerLibrary.Events
{
    public class CreateBoardEvent
    {
        public BoardBody Board { get; set; }
        public Guid CreatorId { get; set; }
    }
}