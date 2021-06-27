using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DirLinkerConfig
{
    public static class DirLinkerInfo
    {
        public const string ApplyConfigArgument = "--applyConfig";
    }

    public class Configuration
    {
        public static Configuration Instance { get; private set; }
        public static readonly string DataDir;
        public static readonly string ConfigFile;

        static Configuration()
        {
            DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "org.comroid");
            ConfigFile = Path.Combine(DataDir, "dirLinker.json");
            Directory.CreateDirectory(DataDir);
        }

        [JsonProperty]
        public uint ConfigVersion = 1;
        [JsonProperty]
        public List<LinkDir> LinkDirectories = new List<LinkDir>();

        [JsonIgnore]
        public IEnumerable<LinkBlob> LinkBlobs => LinkDirectories.SelectMany(it => it.Links);

        public static void LoadConfig()
        {
            if (File.Exists(ConfigFile))
            {
                string data = File.ReadAllText(ConfigFile);
                Instance = JsonConvert.DeserializeObject<Configuration>(data) ?? CreateDefaultConfig();
            }
            else Instance = CreateDefaultConfig();

            if (Instance == null)
                throw new InvalidDataException("Could not load configuration");
        }

        public static void SaveConfig()
        {
            //ApplyConfigFromUI();
            var data = JsonConvert.SerializeObject(Instance);
            File.WriteAllText(ConfigFile, data);
            Debug.WriteLine("Config was saved. Data: " + data);
        }

        public static Configuration CreateDefaultConfig()
        {
            var defaults = new Configuration();
            return defaults;
        }

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
