using System;

namespace BlobConsoleUpload
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

            Console.WriteLine(string.Empty);
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
}
