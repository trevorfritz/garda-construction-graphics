using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace BlobConsoleUpload
{
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

        public void Run(bool delete = false)
        {
            var file = UserSelectFile();
            if (file != null)
            {
                var args = JsonConvert.DeserializeObject<UploadArguments>(File.ReadAllText(file));
                var uploader = new Uploader();
                uploader.Run(args, delete);
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
}
