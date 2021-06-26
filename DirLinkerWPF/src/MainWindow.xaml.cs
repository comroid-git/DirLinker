using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DirLinkerWPF.src;
using Newtonsoft.Json;
using SymbolicLinkSupport;

namespace DirLinkerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int WindowHeight = 600;
        public const int WindowWidth = 840;
        public readonly string DataDir;
        public readonly string ConfigFile;
        public Configuration Config { get; private set; }
        private DebugOutput _debugOutput;
        private bool _debugExpanded;
        private string _debugBacklog = "";

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            Height = WindowHeight;
            Width = WindowWidth;
            ResizeMode = ResizeMode.CanMinimize;
            DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "org.comroid");
            Directory.CreateDirectory(DataDir);
            ConfigFile = Path.Combine(DataDir, "dirLinker.json");

            try
            {
                LoadConfig();
            }
            catch (Exception e)
            {
                WriteLine("Could not load configuration: " + e);
            }
        }

        private void ApplyConfigToOS()
        {
            if (Config.ConfigVersion != 1)
                throw new InvalidDataException("Unknown configuration Version");
            foreach (var it in Config.LinkDirectories)
            {
                var parentDir = it.Dir;

                if (!parentDir.Exists)
                {
                    WriteLine($"Missing link base directory: {parentDir}; skipping entry");
                    continue;
                }

                foreach (var blob in it.Links)
                {
                    var linkPath = Path.Combine(parentDir.FullName, blob.LinkName);
                    var targetDir = new DirectoryInfo(blob.TargetDirectory);

                    if (!targetDir.Exists)
                    {
                        WriteLine($"Missing link target directory: {targetDir}; skipping entry");
                        continue;
                    }
                    if (!Directory.Exists(linkPath))
                    {
                        WriteLine($"Link directory already exists: {linkPath}; skipping entry");
                        continue;
                    }

                    targetDir.CreateSymbolicLink(linkPath);
                }
            }
        }

        private void AddLinkFromInput()
        {
            var linkDir = new DirectoryInfo(LinkDirInput.Text);
            var linkName = LinkNameInput.Text;
            var link = new DirectoryInfo(Path.Combine(linkDir.FullName, linkName));
            var targetDir = new DirectoryInfo(TargetDirInput.Text);

            if (!linkDir.Exists)
                throw new InvalidOperationException("Link parent directory is missing: " + linkDir.FullName);
            if (link.Exists)
                throw new InvalidOperationException("Link target already exists: " + link.FullName);
            if (!targetDir.Exists)
                throw new InvalidOperationException("Link target directory is missing: " + targetDir.FullName);
            
            var dirObj = Config.LinkDirectories.Find(it => it.Directory == linkDir.FullName)
                ?? new Configuration.LinkDir { Dir = linkDir, Links = new List<Configuration.LinkBlob>() };
            if (dirObj.Entry == null)
                dirObj.Entry = new LinkDirEntry(this, dirObj);
            LinkList.Children.Add(dirObj.Entry);
            var linkEntry = dirObj.Entry.GetOrCreateLink(linkName, targetDir);
            WriteLine("Created" + linkEntry);
        }

        private void LoadConfig()
        {
            if (File.Exists(ConfigFile))
            {
                string data = File.ReadAllText(ConfigFile);
                Config = JsonConvert.DeserializeObject<Configuration>(data) ?? CreateDefaultConfig();
            } else Config = CreateDefaultConfig();

            if (Config == null)
                throw new InvalidDataException("Could not load configuration");
        }

        private void SaveConfig()
        {
            var data = JsonConvert.SerializeObject(Config);
            File.WriteAllText(ConfigFile, data);
            WriteLine("Config was saved. Data: " + data);
        }

        private void UpdateLinkList()
        {
            if (Config.ConfigVersion != 1)
                throw new InvalidDataException("Unknown configuration Version");
        }

        private Configuration CreateDefaultConfig()
        {
            var defaults = new Configuration();
            return defaults;
        }

        private void ToggleDebugExpanded()
        {
            if (_debugOutput != null)
            {
                _debugOutput.Close();
                _debugOutput = null;
            }
            else
            {
                _debugOutput = new DebugOutput();
                _debugOutput.Show();
                _debugOutput.WriteLine(_debugBacklog);
            }
        }

        public void StartEditDirectory(LinkDirEntry linkDirEntry)
        {
            LinkList.Children.Remove(linkDirEntry);

        }

        private void WriteLine(object line)
        {
            var str = line.ToString();
            _debugBacklog += str;
            Debug.WriteLine(str);
            _debugOutput?.WriteLine(str);
        }

        private void Button_ApplyConfig(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyConfigToOS();
                SaveConfig();
            }
            catch (Exception ex)
            {
                WriteLine("Could not apply Configuration: " + ex);
            }
        }

        private void Button_ExpandDebug(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleDebugExpanded();
            }
            catch (Exception ex)
            {
                WriteLine("Could not toggle debug window: " + ex);
            }
        }

        private void Button_AddLink(object sender, RoutedEventArgs e)
        {
            try
            {
                AddLinkFromInput();
                UpdateLinkList();
            }
            catch (Exception ex)
            {
                WriteLine("Could not add link: " + ex);
            }
        }

        private void Button_OpenConfig(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveConfig();
                Process.Start("explorer.exe", ConfigFile);
            }
            catch (Exception ex)
            {
                WriteLine("Could not open config: " + ex);
            }
        }
    }
}
