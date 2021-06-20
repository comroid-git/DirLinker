using Newtonsoft.Json;
using SymbolicLinkSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DirLinker
{
    internal static class Models
    {
        public static string GetFullPath(this DirectoryInfo it)
        {
            return Path.GetFullPath(it.ToString());
        }
    }

    public class Configuration
    {
        [JsonProperty]
        public uint ConfigVersion = 1;
        [JsonProperty]
        public List<LinkDir> LinkDirectories;

        public class LinkDir
        {
            [JsonProperty]
            public string Directory;
            [JsonProperty]
            public List<LinkBlob> Links;

            public DirectoryInfo Dir
            {
                get => new DirectoryInfo(Directory);
                set => Directory = value.FullName;
            }
        }

        public class LinkBlob
        {
            private string _linkName;
            [JsonProperty]
            public string LinkName
            {
                get => _linkName ?? TargetDir.Name;
                set => _linkName = value;
            }
            [JsonProperty]
            public string TargetDirectory;

            public DirectoryInfo TargetDir
            {
                get => new DirectoryInfo(TargetDirectory);
                set => TargetDirectory = value.FullName;
            }
        }
    }

    public sealed class LinkMaker
    {
        public DirectoryInfo LinkName;
        public DirectoryInfo LinkTarget;

        public void MakeLink() {
            if (LinkName == null || LinkTarget == null)
                throw new InvalidOperationException("Insufficient parameters");

            string sourceDrive = Path.GetPathRoot(LinkName.GetFullPath());
            string targetDrive = Path.GetPathRoot(LinkTarget.GetFullPath());

            /*
            if (sourceDrive != targetDrive)
                throw new InvalidOperationException("same drive");
            */

            LinkTarget.CreateSymbolicLink(LinkTarget.GetFullPath()));
        }
    }
}
