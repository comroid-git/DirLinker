using System;
using System.Diagnostics;
using System.IO;
using DirLinkerConfig;
using SymbolicLinkSupport;

namespace HardLinkTool
{
    public class HardLinkTool
    {
        public static Configuration Config => Configuration.Instance;

        public static void Main(string[] args)
        {
            try
            {
                Debug.WriteLine("Arguments: " + string.Join(' ', args));
                Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Internal Error: " + ex);
            }
            finally
            {
                Console.WriteLine("Press any Key to exit");
                Console.ReadKey();
            }
        }

        private static void Run(string[] args)
        {
            if (args[0].Equals(DirLinkerInfo.ApplyConfigArgument))
            {
                Configuration.LoadConfig();
                ApplyConfig();
                return;
            }

            // handle other arguments
        }

        private static void ApplyConfig()
        {
            Console.WriteLine("Applying Configuration! Link Directories: " + Config.LinkDirectories.Count);


            if (Config.ConfigVersion != 1)
                throw new InvalidDataException("Unknown configuration Version");
            foreach (var it in Config.LinkDirectories)
            {
                var parentDir = it.Dir;

                if (!parentDir.Exists)
                {
                    Console.WriteLine($"Missing link base directory: {parentDir}; skipping entry");
                    continue;
                }

                foreach (var blob in it.Links)
                {
                    var linkPath = Path.Combine(parentDir.FullName, blob.LinkName) + Path.DirectorySeparatorChar;
                    var targetDir = new DirectoryInfo(blob.TargetDirectory);

                    if (!targetDir.Exists)
                    {
                        Console.WriteLine($"Missing link target directory: {targetDir}; skipping entry");
                        continue;
                    }
                    if (!Directory.Exists(linkPath))
                    {
                        Console.WriteLine($"Link directory already exists: {linkPath}; skipping entry");
                        continue;
                    }

                    targetDir.CreateSymbolicLink(linkPath);
                }
            }
        }
    }
}