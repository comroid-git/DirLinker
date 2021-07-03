using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DirLinkerConfig;
using Microsoft.VisualBasic.FileIO;
using SymbolicLinkSupport;

namespace HardLinkTool
{
    public class HardLinkTool
    {
        public static Configuration Config => Configuration.Instance;
        public static bool HaltOnErrorOnly { get; private set; }

        public static void Main(string[] args)
        {
            try
            {
                Debug.WriteLine("Arguments: " + string.Join(' ', args));
                HaltOnErrorOnly = args.Contains(DirLinkerInfo.HaltOnErrorOnly);
                Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Internal Error: " + ex);
                WaitForUserInput();
            }

            if (HaltOnErrorOnly) 
                return;
            WaitForUserInput();
        }

        private static void WaitForUserInput()
        {
            Console.WriteLine("Press any Key to exit");
            Console.ReadKey();
        }

        private static void Run(string[] args)
        {
            if (args.Contains(DirLinkerInfo.ApplyConfigArgument))
            {
                Configuration.LoadConfig();
                ApplyConfig();
            }

            // handle other arguments
        }

        private static void ApplyConfig()
        {
            Console.WriteLine("Applying Configuration! Link Directories: " + Config.LinkDirectories.Count);


            if (Config.ConfigVersion != 1)
                throw new InvalidDataException("Unknown configuration Version");

            foreach (var linkDir in Config.LinkDirectories)
            {
                if (!linkDir.Enabled)
                {
                    Console.WriteLine("Skipping disabled directory " + linkDir.Directory);
                    continue;
                }

                foreach (var linkBlob in linkDir.Links)
                {
                    if (!linkBlob.Enabled)
                    {
                        Console.WriteLine("Skipping disabled entry " + linkBlob.LinkName);
                        continue;
                    }

                    try
                    {
                        new ApplicationBlob(linkDir, linkBlob).Apply();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An exception occurred: " + ex);
                    }
                }
            }

            Console.WriteLine("Done!");
        }
    }

    internal sealed class ApplicationBlob
    {
        public Configuration.LinkDir Dir { get; }
        internal Configuration.LinkBlob Blob { get; }
        internal DirectoryInfo Base => Dir.Dir;
        internal DirectoryInfo Link
        {
            get
            {
                var dirName = Path.Combine(Base.FullName, Blob.LinkName);
                if (!dirName.EndsWith(Path.DirectorySeparatorChar))
                    dirName += Path.DirectorySeparatorChar;
                return new DirectoryInfo(dirName);
            }
        }
        internal DirectoryInfo Target => Blob.TargetDir;

        internal ApplicationBlob(Configuration.LinkDir dir, Configuration.LinkBlob blob)
        {
            Dir = dir;
            Blob = blob;
            Console.WriteLine($"Configuring symlink: {Link.FullName} --> {Target.FullName}");
        }

        internal void Apply()
        {
            if (Link.Exists)
            {
                // directory exists
                if (Link.IsSymbolicLink())
                {
                    // nothing to do
                    Console.WriteLine("Nothing to do");
                    return;
                }
                // move the directory to target position and create symlink
                Console.WriteLine($"Need to move link source {Link.FullName} to target directory {Target.FullName}");
                FileSystem.CopyDirectory(Link.FullName, Target.FullName);
                CreateSymlink(true);
                return;
            }
            // only create symlink
            CreateSymlink();
        }

        private void CreateSymlink(bool deleteIfPresent = false)
        {
            if (!Target.Exists)
            {
                Console.WriteLine($"Could not create link {Blob.LinkName}; target {Target.FullName} does not exist");
                return;
            }

            if (Link.Exists && !Link.IsSymbolicLink() && deleteIfPresent)
            {
                Console.WriteLine($"Need to delete existing directory: " + Link.FullName);
                Link.Delete(true);
            }
            Console.WriteLine($"Creating Symbolic Link: {Link.FullName} --> {Target.FullName}");
            Target.CreateSymbolicLink(Link.FullName);
        }
    }
}