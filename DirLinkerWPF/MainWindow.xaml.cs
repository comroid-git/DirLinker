using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using SymbolicLinkSupport;

namespace DirLinkerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly string DataDir;
        public readonly string ConfigFile;
        public readonly string ConfigFile;
        private bool _debugExpanded;

        public Configuration Config { get; private set; }

        public MainWindow()
        {
            DataContext = this;
            SetWindowSize(840, 600);
            DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "org.comroid");
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

        internal void WriteLine(object line)
        {
            Debug.WriteLine("Debug Line: " + line);
            DebugOutput.Text += '\n' + line.ToString();
            DebugScroll.ChangeView(0, DebugScroll.ScrollableHeight, 1);
        }
        
        private void ApplyConfig()
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

        private void ToggleDebugExpanded()
        {
            var offset = (_debugExpanded = !_debugExpanded) ? 180 : 0;
            DebugRow.Height = new GridLength(offset);
            SetWindowSize((int)ApplicationView.PreferredLaunchViewSize.Width, (int)(ApplicationView.PreferredLaunchViewSize.Height + offset));
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
            var linkObj = new Configuration.LinkBlob { LinkName = linkName, TargetDir = targetDir };
            dirObj.Links.Add(linkObj);
        }

        private async Task<StorageFile> GetConfig()
        {
            return await StorageFolder.GetFileAsync("dirLinker.json")
                   ?? await StorageFolder.CreateFileAsync("dirLinker.json");
        }

        private async void LoadConfig()
        {
            StorageFile config = await GetConfig();
            string data = await FileIO.ReadTextAsync(config);
            Config = JsonConvert.DeserializeObject<Configuration>(data) ?? CreateDefaultConfig();

            if (Config == null)
                throw new InvalidDataException("Could not load configuration");
        }

        private async void SaveConfig()
        {
            StorageFile config = await GetConfig();
            await FileIO.WriteTextAsync(config, JsonConvert.SerializeObject(Config));
        }

        private Configuration CreateDefaultConfig()
        {
            var defaults = new Configuration();
            return defaults;
        }

        private void Button_ApplyConfig(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyConfig();
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
            }
            catch (Exception ex)
            {
                WriteLine("Could not add link: " + ex);
            }
        }
    }
}
