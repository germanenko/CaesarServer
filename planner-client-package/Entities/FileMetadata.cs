using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public record FileMetadata(
        string Id,
        string Name,
        string Url,
        string Md5,
        long Size,
        DateTime LastModified
    );
}
