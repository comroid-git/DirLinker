using System.Collections.Generic;
using System.IO;
using DirLinkerWPF.src;
using Newtonsoft.Json;

namespace DirLinkerWPF
{
    public class Configuration
    {
        [JsonProperty]
        public uint ConfigVersion = 1;
        [JsonProperty]
        public List<LinkDir> LinkDirectories = new List<LinkDir>();

        public class LinkDir
        {
            [JsonProperty]
            public string Directory;
            [JsonProperty]
            public List<LinkBlob> Links = new List<LinkBlob>();

            internal DirectoryInfo Dir
            {
                get => new DirectoryInfo(Directory);
                set => Directory = value.FullName;
            }

            internal LinkDirEntry Entry;
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

            internal DirectoryInfo TargetDir
            {
                get => new DirectoryInfo(TargetDirectory);
                set => TargetDirectory = value.FullName;
            }

            internal LinkBlobEntry Entry;
        }
    }
}
