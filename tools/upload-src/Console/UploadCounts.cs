using System;
using System.Diagnostics;

namespace BlobConsoleUpload
{
    public class UploadCounts
    {
        public int FileCountToUpload { get; set; }
        public int FileCountUploaded { get; set; }
        public Stopwatch Duration { get; set; }
        public TimeSpan LastUserUpdate { get; set; }
    }
}
