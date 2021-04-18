namespace BlobConsoleUpload
{
    public class UploadArguments
    {
        public string Name { get; set; } = string.Empty;
        public string FromPath { get; set; } = string.Empty;
        public string ToConnectionString { get; set; } = string.Empty;
        public string ToContainer { get; set; } = string.Empty;
    }
}
