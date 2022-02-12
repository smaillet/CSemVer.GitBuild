using System;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

using Ubiquity.NET.CSemVer;

namespace CSemVer.Build.Tasks
{
    public class GetBuildIndexFromTime
        : Task
    {
        [Required]
        public DateTime TimeStamp { get; set; }

        [Output]
        public string BuildIndex { get; private set; }

        public override bool Execute( )
        {
            BuildIndex = TimeStamp.ToBuildIndex();
            return true;
        }
    }
}
