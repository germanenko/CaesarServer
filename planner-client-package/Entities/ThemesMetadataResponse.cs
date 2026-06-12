using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace planner_client_package.Entities
{
    public record ThemesMetadataResponse(
        int TotalFiles,
        List<FileMetadata> Files,
        DateTime GeneratedAt,
        string Version
    );
}
