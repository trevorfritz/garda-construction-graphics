using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Fritz.Core;

namespace BlobConsoleUpload
{
    public class Uploader
    {
        UploadArguments arguments;
        DirectoryInfo fromRoot;
        BlobServiceClient toService;
        BlobContainerClient toContainer;
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
                CreateMainContainer();
                counts.Duration = Stopwatch.StartNew();
                UploadFolder(fromRoot);
                Console.WriteLine("done");
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
            toService = new BlobServiceClient(arguments.ToConnectionString);
            counts = new UploadCounts();
            Console.WriteLine("Uploading blobs:");
            Console.WriteLine($"  from {fromRoot.FullName}");
            Console.WriteLine($"  to {toService.AccountName}/{arguments.ToContainer}");
        }

        private void CreateMainContainer()
        {
            toContainer = toService.GetBlobContainerClient(arguments.ToContainer);
            if (toContainer.Exists())
            {
                toContainer.Delete();
                Console.WriteLine("  Delete container operation started (may take several minutes)");
            }
            int failedCount = 0;
            while (failedCount >= 0)
            {
                Console.Write("  ...creating container...");
                try
                {
                    var result = toContainer.Create(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
                    Console.WriteLine("OK");
                    failedCount = -1;
                }
                catch (Exception)
                {
                    failedCount++;
                    var spinner = default(string);
                    switch (failedCount % 4)
                    {
                        case 1:
                            spinner = "/";
                            break;
                        case 2:
                            spinner = "-";
                            break;
                        case 3:
                            spinner = "\\";
                            break;
                        default:
                            spinner = "|";
                            break;
                    }
                    Console.Write($"\r{spinner}");
                }

                if (failedCount == 1000) throw new ApplicationException("Timeout waiting for container to be deleted.");

                Thread.Sleep(500);
            }
        }

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
            var properties = toService.GetProperties();
            if (properties.Value.DefaultServiceVersion != targetVersion)
            {
                var previousVersion = properties.Value.DefaultServiceVersion;
                properties.Value.DefaultServiceVersion = targetVersion;
                toService.SetProperties(properties);
                Console.WriteLine($"  Upgraded API version: {previousVersion} --> {targetVersion}");
            }
            else
            {
                Console.WriteLine($"  Current API version: {properties.Value.DefaultServiceVersion}");
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
            Console.WriteLine($"  Found {counts.FileCountToUpload} file(s) to upload");
        }

        private void UploadFolder(DirectoryInfo from)
        {
            foreach (var file in from.GetFiles())
            {
                var blobName = $"{GetContainerPathFromFolder(from)}/{Path.GetFileName(file.FullName)}";
                var blob = toContainer.GetBlobClient(blobName);
                blob.Upload(
                    file.OpenRead(), 
                    new BlobHttpHeaders { ContentType = MediaTypes.FromExtension(Path.GetExtension(file.FullName)).ToString() }
                );
                counts.FileCountUploaded++;
                //if ((counts.Duration.Elapsed - counts.LastUserUpdate) >= TimeSpan.FromSeconds(1) || counts.LastUserUpdate == TimeSpan.Zero)
                //{
                    Console.Write($"\r  ...uploading ({counts.FileCountUploaded} of {counts.FileCountToUpload})...");
                //}
                counts.LastUserUpdate = counts.Duration.Elapsed;
            }

            foreach (var subFolder in from.GetDirectories())
            {
                UploadFolder(subFolder);
            }
        }

        //private void SetContentType(string blobName, string extension)
        //{
        //    item.Properties.ContentType = "";
        //    var blob = toContainer.GetBlobClient(blobName);
        //    //blob.SetHttpHeaders = new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType =  } }
        //}

        private string GetContainerPathFromFolder(DirectoryInfo folder)
        {
            var relative = Path.GetRelativePath(fromRoot.Parent.FullName, folder.FullName);
            relative = relative.Replace('\\', '/');
            //var container = toService.GetBlobContainerClient(relative);
            //container.CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.Blob);
            //relative = "/" + relative;
            return relative;
        }
    }
}
