using System;
using System.Diagnostics;
using System.IO;
using Azure.Storage.Blobs;
using Newtonsoft.Json;

namespace GardaUploadConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var actions = new Actions();

            if (args.Length == 0)
            {
                DisplayHelp();
                return;
            }

            if (args[0] == "new")
            {
                actions.New();
            }
            else if (args[0] == "list")
            {
                actions.List();
            }
            else if (args[0] == "openfolder")
            {
                actions.OpenFolder();
            }
            else if (args[0] == "open")
            {
                actions.Open();
            }
            else if (args[0] == "run")
            {
                actions.Run();
            }

            Console.WriteLine("OK");
        }

        public static void DisplayHelp()
        {
            Console.WriteLine("ARGS:");
            Console.WriteLine("================================================================================");
            Console.WriteLine("  new - creates a new empty template and opens it");
            Console.WriteLine("  openfolder - opens folder containing configuration files (so you can manage them)");
            Console.WriteLine("  open - opens an argument file so you can edit it");
            Console.WriteLine("  list - opens folder containing configuration files (so you can manage them)");
            Console.WriteLine("  run - executes an upload into blob account specified by the file");
        }
    }

    public class Actions
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GardaUploader");

        public void New()
        {
            Directory.CreateDirectory(path);
            var n = Path.Combine(path, "new_file.json");
            var args = new UploadArguments();
            WriteObject(n, args);
            var proc = Process.Start("notepad.exe", n);
            proc.WaitForExit();
            args = JsonConvert.DeserializeObject<UploadArguments>(File.ReadAllText(n));
            File.Copy(n, args.Name);
            File.Delete(n);
        }

        public void List()
        {
            Console.WriteLine(path);
            foreach (var file in Directory.GetFiles(path))
            {
                Console.WriteLine($"    {Path.GetFileName(file)}");
            }
        }

        public void OpenFolder()
        {
            Process.Start("explorer.exe", path);
        }

        public void Open()
        {
            var file = UserSelectFile();
            if (file != null)
            {
                var proc = Process.Start("notepad.exe", file);
                proc.WaitForExit();
            }
        }

        public void Clear()
        {
            File.Delete(path);
        }

        public void Run()
        {
            var file = UserSelectFile();
            if (file != null)
            {
                var args = JsonConvert.DeserializeObject<UploadArguments>(File.ReadAllText(file));
                var uploader = new Uploader();
                uploader.Run(args);
            }
        }

        private string UserSelectFile()
        {
            var files = Directory.GetFiles(path);
            int i = 1;
            foreach (var f in files)
            {
                Console.WriteLine($"    {i} - {Path.GetFileName(f)}");
                i++;
            }
            Console.WriteLine("0 to exit");
            Console.Write("Which file ?");
            var key = Console.ReadLine();
            var fileSelected = int.Parse(key);
            if (fileSelected == 0) return null;

            return  files[fileSelected - 1];
        }

        private void WriteObject(string path, UploadArguments args)
        {
            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            if (args.FromPath != null ) args.FromPath = args.FromPath.Replace('\\', '/');
            File.WriteAllText(path, JsonConvert.SerializeObject(args, settings));
        }
    }

    
    public class Uploader
    {
        UploadArguments arguments;
        BlobServiceClient toRoot;
        DirectoryInfo fromRoot;
        UploadCounts counts;

        public Uploader()
        {
        }

        public void Run(UploadArguments arguments)
        {
            try
            {
                Init(arguments);
                CountFiles(fromRoot);
                DisplayPreCounts();
                FixVersion();
                //CreateRootContainer();
                UploadFolder(fromRoot, GetContainerFromFolder(fromRoot));
            }
            catch (Exception exception)
            {
                Console.WriteLine("ERROR: Problem uploading '" + exception.Message + "'");
                Console.WriteLine(exception.ToString());
            }
        }

        private void Init(UploadArguments arguments)
        {
            this.arguments = arguments;
            fromRoot = new DirectoryInfo(arguments.FromPath);
            toRoot = new BlobServiceClient(arguments.ToConnectionString);
            counts = new UploadCounts();
            Console.WriteLine("Uploading blobs:");
            Console.WriteLine($"  from {fromRoot.FullName}");
            Console.WriteLine($"  to {toRoot.AccountName}");
        }

        //private void CreateRootContainer()
        //{
        //    toContainerRoot = toRoot.GetBlobContainerClient(arguments.ToContainer);
        //    var createResult = toContainerRoot.CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
        //    if (createResult != null)
        //    {
        //        Console.WriteLine($"Container '{toContainerRoot.Name}' created with public access");
        //    }
        //}

        /// <summary>
        /// Sets the default version of the service API.
        /// </summary>
        /// <remarks>
        /// When the blob service receives an anonymous request from a client that does not
        /// have a version specified it falls back to 2009-09-19.  This version does not
        /// handle video streaming well:  
        ///     1) takes a long time for video to load 
        ///     2) finally after loading seek operations don't work
        /// Setting the version to anything over 2013-08-15 fixes this problem  This method
        /// sets to the latest at the time of this writing.
        /// 
        /// For documenation on all service API version: https://docs.microsoft.com/en-us/rest/api/storageservices/previous-azure-storage-service-versions
        /// Fix was found here under "Check your Storage Version": https://blog.thoughtstuff.co.uk/2014/01/streaming-mp4-video-files-in-azure-storage-containers-blob-storage/
        /// </remarks>
        private void FixVersion()
        {
            var targetVersion = "2020-06-12";
            var properties = toRoot.GetProperties();
            if (properties.Value.DefaultServiceVersion != targetVersion)
            {
                var previousVersion = properties.Value.DefaultServiceVersion;
                properties.Value.DefaultServiceVersion = targetVersion;
                toRoot.SetProperties(properties);
                Console.WriteLine($"Upgraded API version: {previousVersion} --> {targetVersion}");
            }
            else
            {
                Console.WriteLine($"Current API version: {properties.Value.DefaultServiceVersion}");
            }
        }

        private void CountFiles(DirectoryInfo folder)
        {
            counts.FileCountToUpload += folder.GetFiles().Length;
            foreach (var sub in folder.GetDirectories())
            {
                CountFiles(sub);
            }
        }

        private void DisplayPreCounts()
        {
            Console.WriteLine($"Found {counts.FileCountToUpload} file(s) to upload");
        }

        private void UploadFolder(DirectoryInfo from, BlobContainerClient toPath)
        {
            foreach (var file in from.GetFiles())
            {
                toPath.UploadBlob(Path.GetFileName(file.FullName), file.OpenRead());
                Console.WriteLine($"Uploaded '{file.Name}'");
            }

            foreach (var subFolder in from.GetDirectories())
            {
                UploadFolder(subFolder, GetContainerFromFolder(subFolder));
            }
        }

        private BlobContainerClient GetContainerFromFolder(DirectoryInfo folder)
        {
            var relative = Path.GetRelativePath(fromRoot.Parent.FullName, folder.FullName);
            relative = relative.Replace('\\', '/');
            var container = toRoot.GetBlobContainerClient(relative);
            container.CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
            //relative = "/" + relative;
            return container;
        }
    }

    public class UploadArguments
    {
        public string Name { get; set; } = string.Empty;
        public string FromPath { get; set; } = string.Empty;
        public string ToConnectionString { get; set; } = string.Empty;
        //public string ToContainer { get; set; } = string.Empty;
    }

    public class UploadCounts
    {
        public int FileCountToUpload { get; set; }
    }
}
