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

            foreach (var linkDir in Config.LinkDirectories)
            foreach (var linkBlob in linkDir.Links)
                new ApplicationBlob(linkDir, linkBlob).Apply();
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
        }

        internal void Apply()
        {
            if (Link.Exists)
            {
                // directory exists
                if (Link.IsSymbolicLink())
                {
                    // nothing to do
                    Console.WriteLine("Nothing to do for link " + Blob.LinkName);
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