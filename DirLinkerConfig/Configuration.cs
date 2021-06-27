using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DirLinkerConfig
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
            
            [JsonIgnore]
            public DirectoryInfo Dir
            {
                get => new DirectoryInfo(Directory);
                set => Directory = value.FullName;
            }

            [JsonIgnore]
            public object Entry;
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

            [JsonIgnore]
            public DirectoryInfo TargetDir
            {
                get => new DirectoryInfo(TargetDirectory);
                set => TargetDirectory = value.FullName;
            }

            [JsonIgnore]
            public object Entry;
        }
    }
}
