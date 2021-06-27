using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DirLinkerConfig;

namespace DirLinkerWPF
{
    /// <summary>
    /// Interaction logic for DirLinker.xaml
    /// </summary>
    public partial class DirLinker : Window
    {
        public const int WindowHeight = 600;
        public const int WindowWidth = 840;
        public Configuration Config => Configuration.Instance;
        private Dictionary<string, Configuration.LinkDir> _blobs = new Dictionary<string, Configuration.LinkDir>();

        public DirLinker()
        {
            Icon = new BitmapImage(new Uri("https://cdn.comroid.org/img/logo-clean.ico"));
            DataContext = this;
            InitializeComponent();
            Height = WindowHeight;
            Width = WindowWidth;
            ResizeMode = ResizeMode.CanMinimize;
            Closing += (sender, args) => Configuration.SaveConfig();

            try
            {
                Configuration.LoadConfig();
                UpdateLinkList();
                CleanupConfig();
            }
            catch (Exception e)
            {
                PromptLine("Could not load configuration: " + e);
            }
        }

        private void ApplyConfigToOS()
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = "HardLinkTool.exe",
                Arguments = DirLinkerInfo.ApplyConfigArgument,
                UseShellExecute = false,
                Verb = "runas"
            };

            Process.Start(startInfo);
        }

        private void AddLinkFromInput()
        {
            var linkDirStr = LinkDirInput.Text;
            var linkDir = new DirectoryInfo(linkDirStr.EndsWith(Path.DirectorySeparatorChar) ? linkDirStr : linkDirStr + Path.DirectorySeparatorChar);
            var targetStr = TargetDirInput.Text;
            var targetDir = new DirectoryInfo(targetStr.EndsWith(Path.DirectorySeparatorChar) ? targetStr : targetStr + Path.DirectorySeparatorChar);

            /*
            if (!linkDir.Exists)
                throw new InvalidOperationException("Link parent directory is missing: " + linkDir.FullName);
            if (link.Exists)
                throw new InvalidOperationException("Link target already exists: " + link.FullName);
            if (!targetDir.Exists)
                throw new InvalidOperationException("Link target directory is missing: " + targetDir.FullName);
            */

            var dirEntry = GetOrCreateDir(linkDir);
            var linkEntry = dirEntry.GetOrCreateLink(LinkNameInput.Text, targetDir);
        }

        private LinkDirEntry Add(Configuration.LinkDir blob)
        {
            var entry = new LinkDirEntry(this, blob);
            LinkList.Children.Add(entry);
            blob.Entry = entry;
            _blobs[blob.Directory] = blob;
            Config.LinkDirectories.Add(blob);
            return entry;
        }

        private LinkDirEntry GetOrCreateDir(DirectoryInfo dir)
        {
            _blobs.TryGetValue(dir.FullName, out Configuration.LinkDir add);
            if (add != null)
                return add.Entry as LinkDirEntry;
            return Add(new Configuration.LinkDir { Dir = dir });
        }

        private void CleanupConfig()
        {
            Debug.WriteLine("Config before cleanup: " + Config.LinkDirectories.Count);

            for (int existingIndex = 0;
                existingIndex < Config.LinkDirectories.Count; 
                existingIndex++)
            {
                var existing = Config.LinkDirectories[existingIndex];

                for (int possibleDuplicateIndex = existingIndex + 1;
                    possibleDuplicateIndex < Config.LinkDirectories.Count;
                    possibleDuplicateIndex++)
                {
                    var possibleDuplicate = Config.LinkDirectories[possibleDuplicateIndex];

                    if (existing.Directory.Equals(possibleDuplicate.Directory))
                        Config.LinkDirectories.RemoveAt(possibleDuplicateIndex);
                }
            }
            
            Debug.WriteLine("Config after cleanup: " + Config.LinkDirectories.Count);
        }

        private void UpdateLinkList()
        {
            if (Config.ConfigVersion != 1)
                throw new InvalidDataException("Unknown configuration Version");

            var dirs = new List<Configuration.LinkDir>(Config.LinkDirectories.Where(it => !_blobs.Values.Contains(it)));

            foreach (var each in dirs)
            {
                var entry = GetOrCreateDir(each.Dir);

                var blobs = new List<Configuration.LinkBlob>(each.Links.Where(it => !entry.Blobs.Values.Contains(it)));
                
                foreach (var blob in blobs)
                {
                    var blobEntry = entry.GetOrCreateLink(blob.LinkName, blob.TargetDir);
                }
            }
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
                Configuration.SaveConfig();
                ApplyConfigToOS();
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
                Configuration.SaveConfig();
                Process.Start("explorer.exe", Configuration.ConfigFile);
            }
            catch (Exception ex)
            {
                PromptLine("Could not open config: " + ex);
            }
        }
    }
}
