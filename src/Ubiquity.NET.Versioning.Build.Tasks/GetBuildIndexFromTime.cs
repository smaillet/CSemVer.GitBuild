using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Ubiquity.NET.Versioning.Tasks
{
    public class GetBuildIndexFromTime
        : Task
    {
        [Required]
        public DateTime TimeStamp { get; set; }

        [Output]
        public string? BuildIndex { get; private set; }

        public override bool Execute( )
        {
            BuildIndex = new DateTimeOffset(TimeStamp.ToUniversalTime()).ToBuildIndex();
            return true;
        }
    }
}
