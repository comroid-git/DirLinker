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
        public const string HaltOnErrorOnly = "--haltOnErrorOnly";
    }

    public class Configuration : IUpdateable<Configuration>
    {
        public static Configuration Instance { get; private set; }
        public static readonly string DataDir;
        public static readonly string ConfigFile;
        [JsonIgnore] 
        public IEntryProducer Producer;

        static Configuration()
        {
            DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "org.comroid");
            ConfigFile = Path.Combine(DataDir, "dirLinker.json");
            Directory.CreateDirectory(DataDir);
            LoadConfig();
        }

        [JsonProperty]
        public uint ConfigVersion = 1;
        [JsonProperty]
        public bool PauseConsole;
        [JsonProperty]
        public List<LinkDir> LinkDirectories = new List<LinkDir>();

        [JsonIgnore]
        public IEnumerable<LinkBlob> LinkBlobs => LinkDirectories.SelectMany(it => it.Links);

        public LinkDir GetOrCreate(string path)
        {
            var it = LinkDirectories.FirstOrDefault(e => e.Directory.Equals(path));
            if (it == null)
                it = new LinkDir { Directory = path };
            return it;
        }

        public static void LoadConfig()
        {
            Configuration newData;
            if (File.Exists(ConfigFile))
            {
                string data = File.ReadAllText(ConfigFile);
                newData = JsonConvert.DeserializeObject<Configuration>(data) ?? CreateDefaultConfig();
            }
            else newData = CreateDefaultConfig();

            if (newData == null)
                throw new InvalidDataException("Could not load configuration");
            if (Instance == null) 
                Instance = newData;
            Instance.UpdateFrom(newData);
        }

        public void UpdateFrom(Configuration newData = null)
        {
            if (newData != null)
            {
                ConfigVersion = newData.ConfigVersion;
                PauseConsole = newData.PauseConsole;
                var news = new List<LinkDir>();
                foreach (var newDir in newData.LinkDirectories)
                {
                    var newDupe = LinkDirectories.FirstOrDefault(it => it.Directory.Equals(newDir.Directory));
                    if (newDupe == null)
                        news.Add(newDir);
                    else newDupe.UpdateFrom(newDir);
                }

                foreach (var newDir in news)
                    LinkDirectories.Add(newDir);
            }

            foreach (var linkDir in LinkDirectories.Where(it => it.Entry == null))
            {
                var entry = Producer?.CreateDirEntry(linkDir);
                linkDir.Entry = entry;
            }

            foreach (var each in LinkDirectories)
            {
                each.Config = this;
                each.Producer = Producer;
            }
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

        public class LinkDir : IUpdateable<LinkDir>
        {
            [JsonIgnore] 
            public Configuration Config;
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
            public IEntryProducer Producer;
            [JsonIgnore]
            public ILinkDirEntry Entry;

            public LinkBlob GetOrCreate(string name, DirectoryInfo directory)
            {
                var it = Links.FirstOrDefault(e => e.LinkName.Equals(name));
                if (it == null)
                    it = new LinkBlob { LinkName = name, TargetDir = directory };
                return it;
            }

            public void UpdateFrom(LinkDir newData)
            {
                if (newData != null)
                {
                    Directory = newData.Directory;
                    var news = new List<LinkBlob>();
                    foreach (var newBlob in newData.Links)
                    {
                        var newDupe = Links.FirstOrDefault(it => it.LinkName.Equals(newBlob.LinkName));
                        if (newDupe == null)
                            news.Add(newBlob);
                        else newDupe.UpdateFrom(newBlob);
                    }

                    foreach (var newBlob in news)
                        Links.Add(newBlob);
                }

                foreach (var linkBlob in Links.Where(it => it.Entry == null))
                {
                    var entry = Producer?.CreateBlobEntry(this, linkBlob);
                    linkBlob.Entry = entry;
                }
                
                foreach (var each in Links) 
                    each.DirBlob = this;
            }
        }

        public class LinkBlob : IUpdateable<LinkBlob>
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
            public LinkDir DirBlob;
            [JsonIgnore]
            public ILinkBlobEntry Entry;

            public void UpdateFrom(LinkBlob newData)
            {
                if (newData != null)
                {
                    LinkName = newData.LinkName;
                    TargetDirectory = newData.TargetDirectory;
                }
            }
        }
    }

    public interface IUpdateable<T>
    {
        void UpdateFrom(T newData);
    }
    
    public interface ILinkDirEntry
    {
        bool IsDemo { get; }
        string LinkDirName { get; set; }
    }
    
    public interface ILinkBlobEntry
    {
        string LinkName { get; set; }
        string TargetName { get; set; }
    }

    public interface IEntryProducer
    {
        ILinkDirEntry CreateDirEntry(Configuration.LinkDir blob);
        ILinkBlobEntry CreateBlobEntry(Configuration.LinkDir dir, Configuration.LinkBlob blob);
    }
}
