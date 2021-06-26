using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        private bool _debugExpanded;

        public MainWindow()
        {
            DataContext = this;
            Height = WindowHeight;
            Width = WindowWidth;
            ResizeMode = ResizeMode.CanMinimize;
            DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "org.comroid");
            var dir = new DirectoryInfo(DataDir);
            if (!dir.Exists) 
                dir.Create();
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
            //DebugOutput.Text += '\n' + line.ToString();
            //DebugScroll.ChangeView(0, DebugScroll.ScrollableHeight, 1);
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
            Height = WindowHeight + offset;
            Width = WindowHeight + offset;
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

        private void LoadConfig()
        {
            string data = File.ReadAllText(ConfigFile);
            Config = JsonConvert.DeserializeObject<Configuration>(data) ?? CreateDefaultConfig();

            if (Config == null)
                throw new InvalidDataException("Could not load configuration");
        }

        private void SaveConfig()
        {
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(Config));
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

        // taken from https://stackoverflow.com/questions/636383/how-can-i-find-wpf-controls-by-name-or-type
        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }
    }
}
