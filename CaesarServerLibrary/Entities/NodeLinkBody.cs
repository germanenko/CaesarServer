using CaesarServerLibrary.Enums;
using System;

namespace CaesarServerLibrary.Entities
{
    public class NodeLinkBody
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public Guid ChildId { get; set; }
        public RelationType RelationType { get; set; }
    }
}
