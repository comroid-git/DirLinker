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
using System.Windows.Media.Imaging;
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

        public MainWindow()
        {
            Icon = new BitmapImage(new Uri("https://cdn.comroid.org/img/logo-clean.ico"));
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
                PromptLine("Could not load configuration: " + e);
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
                    PromptLine($"Missing link base directory: {parentDir}; skipping entry");
                    continue;
                }

                foreach (var blob in it.Links)
                {
                    var linkPath = Path.Combine(parentDir.FullName, blob.LinkName);
                    var targetDir = new DirectoryInfo(blob.TargetDirectory);

                    if (!targetDir.Exists)
                    {
                        PromptLine($"Missing link target directory: {targetDir}; skipping entry");
                        continue;
                    }
                    if (!Directory.Exists(linkPath))
                    {
                        PromptLine($"Link directory already exists: {linkPath}; skipping entry");
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

            var dirEntry = GetOrCreateDir(linkDir);
            var linkEntry = dirEntry.GetOrCreateLink(linkName, targetDir);
            PromptLine("Created " + linkEntry);
        }

        private LinkDirEntry Add(Configuration.LinkDir linkDir)
        {
            var yield = new LinkDirEntry(this, linkDir);
            LinkList.Children.Add(linkDir.Entry = yield);
            return yield;
        }

        private LinkDirEntry GetOrCreateDir(DirectoryInfo dir)
        {
            // todo Always creates
            return LinkList.Children.Cast<LinkDirEntry>()
                       .FirstOrDefault(it => it.Blob.Dir == dir)
                   ?? Add(new Configuration.LinkDir {Dir = dir});
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
            Debug.WriteLine("Config was saved. Data: " + data);
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
        
        public void StartEditDirectory(LinkDirEntry linkDirEntry)
        {
            throw new NotImplementedException();
        }

        private void PromptLine(object line)
        {
            var str = line.ToString();

            new Window
            {
                Height = 130,
                Width = 400,
                Content = new TextBlock
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    TextAlignment = TextAlignment.Center,
                    Text = str
                }
            }.ShowDialog();

            Debug.WriteLine(str);
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
                PromptLine("Could not apply Configuration: " + ex);
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
                PromptLine("Could not add link: " + ex);
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
                PromptLine("Could not open config: " + ex);
            }
        }
    }
}
