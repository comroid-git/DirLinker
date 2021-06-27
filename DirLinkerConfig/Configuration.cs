using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SymbolicLinkSupport;

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
        }

        [JsonProperty]
        public uint ConfigVersion = 1;
        [JsonProperty]
        public bool PauseConsole;
        [JsonProperty]
        public List<LinkDir> LinkDirectories = new List<LinkDir>();

        [JsonIgnore]
        public IEnumerable<LinkBlob> LinkBlobs => LinkDirectories.SelectMany(it => it.Links);

        [CanBeNull]
        public LinkDir Find(string path)
        {
            return LinkDirectories.FirstOrDefault(e => e.Directory.Equals(path));
        }

        public LinkDir GetOrCreate(string path)
        {
            var dir = Find(path);
            if (dir != null)
            {
                return dir;
            }
            Debug.WriteLine("Warning: Could not find LinkDir " + path);

            var blob = new LinkDir(this, Producer) { Directory = path };
            Add(blob);
            return blob;
        }

        public void Add(LinkDir blob)
        {
            var entry = Producer?.CreateDirEntry(blob);
            blob.Entry = entry;
            LinkDirectories.Add(blob);
            Producer?.AddDirToView(entry);
        }

        public bool Remove(string path)
        {
            var dir = Find(path);
            return dir != null && LinkDirectories.Remove(dir);
        }

        public static void LoadConfig(IEntryProducer producer = null)
        {
            Configuration newData;
            if (File.Exists(ConfigFile))
            {
                string data = File.ReadAllText(ConfigFile);
                newData = JsonConvert.DeserializeObject<Configuration>(data) ?? CreateDefaultConfig();
            }
            else newData = CreateDefaultConfig();

            if (producer != null)
                newData.Producer = producer;
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
                    Add(newDir);
            }

            Producer?.ClearView();
            foreach (var each in LinkDirectories) 
                Producer?.AddDirToView(each.Entry);
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
            public LinkDir(Configuration config, IEntryProducer producer)
            {
                Config = config;
                Producer = producer;
            }

            [JsonIgnore] 
            public readonly Configuration Config;
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
            public readonly IEntryProducer Producer;
            [JsonIgnore]
            public ILinkDirEntry Entry;

            [CanBeNull]
            public LinkBlob Find(string name)
            {
                return Links.FirstOrDefault(e => e.LinkName.Equals(name));
            }
            
            public LinkBlob GetOrCreate(string name, DirectoryInfo directory)
            {
                var find = Find(name);
                if (find != null)
                {
                    return find;
                }
                Debug.WriteLine("Warning: Could not find LinkBlob " + name);

                var blob = new LinkBlob(this) { LinkName = name, TargetDir = directory };
                Add(blob);
                return blob;
            }

            public void Add(LinkBlob blob)
            {
                var entry = Producer?.CreateBlobEntry(this, blob);
                blob.Entry = entry;
                Links.Add(blob);
                Entry?.AddLinkToView(entry);
            }

            public bool Remove(string linkName)
            {
                var link = Find(linkName);
                if (link == null)
                    return false;
                /*
                 TODO: delete link on removed from config
                var lnk = link.Link;
                if (lnk.Exists && lnk.IsSymbolicLink())
                    lnk.Delete(false);
                */
                return Links.Remove(link);
            }

            public void UpdateFrom(LinkDir newData = null)
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
                        Add(newBlob);
                }
                
                Entry?.ClearView();
                foreach (var each in Links) 
                    Entry?.AddLinkToView(each.Entry);
            }
        }

        public class LinkBlob : IUpdateable<LinkBlob>
        {
            public LinkBlob(LinkDir dirBlob)
            {
                DirBlob = dirBlob;
            }

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
            public DirectoryInfo Link
            {
                get
                {
                    var combine = Path.Combine(DirBlob.Directory, LinkName);
                    if (!combine.EndsWith(Path.DirectorySeparatorChar))
                        combine += Path.DirectorySeparatorChar;
                    return new DirectoryInfo(combine);
                }
            }
            [JsonIgnore] 
            public readonly LinkDir DirBlob;
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

    public interface IEntryProducer
    {
        void ReloadView();
        void ClearView();
        void AddDirToView(ILinkDirEntry entry);
        ILinkDirEntry CreateDirEntry(Configuration.LinkDir blob);
        ILinkBlobEntry CreateBlobEntry(Configuration.LinkDir dir, Configuration.LinkBlob blob);
    }

    public interface IUpdateable<in T>
    {
        void UpdateFrom(T newData);
    }
    
    public interface ILinkDirEntry
    {
        void ReloadView();
        void ClearView();
        void AddLinkToView(ILinkBlobEntry entry);
    }
    
    public interface ILinkBlobEntry
    {
    }
}
