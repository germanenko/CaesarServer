using planner_common_package.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_server_package.Stubs
{
    public sealed record TestStub(
    string Key,              // "node.delete.writeDenied"
    ErrorCode Response,      // что подставлять: WriteDenied, VersionMismatch...
    string Reason,           // "Даниил, тест отката удаления"
    DateOnly RemoveOn);      // срок снятия, 2026-11-01
}
