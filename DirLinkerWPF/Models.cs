using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DirLinkerWPF
{
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
}
