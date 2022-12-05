using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DirLinkerConfig;

namespace DirLinkerWPF
{
    /// <summary>
    /// Interaction logic for DirLinker.xaml
    /// </summary>
    public partial class DirLinker : Window, IEntryProducer
    {
        public const string PauseConsolePrefix = "Pause Console?\n";
        public const int WindowHeight = 600;
        public const int WindowWidth = 840;
        public Configuration Config => Configuration.Instance;

        public DirLinker()
        {
            DataContext = this;
            InitializeComponent();
            Height = WindowHeight;
            Width = WindowWidth;
            ResizeMode = ResizeMode.CanMinimize;
            Closing += (sender, args) => Configuration.SaveConfig();

            try
            {
                if (!File.Exists(Configuration.ConfigFile)) 
                    Configuration.SaveConfig();
                Configuration.LoadConfig(this);
                CleanupConfig();
            }
            catch (Exception e)
            {
                PromptText("Could not load configuration: " + e);
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            HaltConsoleBtn.Content = PauseConsolePrefix + (Config.PauseConsole ? "✔ yes" : "❌ no");
            ReloadView();
        }

        private void ApplyConfigToOS()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "HardLinkTool.exe",
                Arguments = Config.PauseConsole ? string.Empty : DirLinkerInfo.HaltOnErrorOnly,
                UseShellExecute = true,
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

            var dirEntry = Config.GetOrCreate(linkDir.FullName);
            var linkEntry = dirEntry.GetOrCreate(LinkNameInput.Text, targetDir);

            LinkDirInput.Text = "";
            LinkDirInput.IsEnabled = true;
            LinkNameInput.Text = "";
            LinkNameInput.IsEnabled = true;
            TargetDirInput.Text = "";
            TargetDirInput.IsEnabled = true;

            ReloadView();
        }

        internal void CleanupConfig()
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

        public void StartAddLink(string forDir)
        {
            LinkDirInput.Text = forDir;
            LinkDirInput.IsEnabled = false;
        }

        public void StartEditDirectory(LinkDirEntry linkDirEntry)
        {
            throw new NotImplementedException();
        }

        private void PromptText(object line)
        {
            var str = line.ToString();

            new Window
            {
                Height = 260,
                Width = 1200,
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
                CleanupConfig();
                Configuration.SaveConfig();
                ApplyConfigToOS();
            }
            catch (Exception ex)
            {
                PromptText("Could not apply Configuration: " + ex);
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
                PromptText("Could not add link: " + ex);
            }
        }

        private void Button_OpenConfig(object sender, RoutedEventArgs e)
        {
            try
            {
                new ConfigPopup(this).ShowDialog();
            }
            catch (Exception ex)
            {
                PromptText("Could not open config: " + ex);
            }
        }

        private void Button_ToggleHaltConsole(object sender, RoutedEventArgs e)
        {
            try
            {
                Config.PauseConsole = !Config.PauseConsole;
                UpdateUI();
            }
            catch (Exception ex)
            {
                PromptText("Could not toggle Halting state: " + ex);
            }
        }

        internal void Button_RemoveDir(LinkDirEntry linkDir)
        {
            try
            {
                Config.Remove(linkDir.Blob.Directory);
                UpdateUI();
            }
            catch (Exception ex)
            {
                PromptText($"Could not remove LinkDir {linkDir.LinkDirName}: " + ex);
            }
        }

        public void Button_RemoveBlob(Configuration.LinkDir linkDir, Configuration.LinkBlob linkBlob)
        {
            try
            {
                Config.Find(linkDir.Directory)?.Remove(linkBlob.LinkName);
                UpdateUI();
            }
            catch (Exception ex)
            {
                PromptText($"Could not remove LinkBlob {linkDir.Directory} - {linkBlob.LinkName}: " + ex);
            }
        }

        public void ReloadView()
        {
            foreach (var each in Config.LinkDirectories.Where(it => it.Entry == null))
                each.Entry = CreateDirEntry(each);
            ClearView();
            foreach (var each in Config.LinkDirectories)
            {
                AddDirToView(each.Entry);
                each.Entry.ReloadView();
            }
        }

        public void ClearView()
        {
            LinkList.Children.Clear();
        }

        public void AddDirToView(ILinkDirEntry entry)
        {
            if (entry is LinkDirEntry value) 
                LinkList.Children.Add(value);

        }

        public ILinkDirEntry CreateDirEntry(Configuration.LinkDir blob)
        {
            return new LinkDirEntry(this, blob);
        }

        public ILinkBlobEntry CreateBlobEntry(Configuration.LinkDir dir, Configuration.LinkBlob blob)
        {
            return new LinkBlobEntry(this, dir, blob);
        }
    }
}
