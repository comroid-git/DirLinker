using Newtonsoft.Json;
using SymbolicLinkSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DirLinker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static readonly FileInfo ConfigFile;
        private bool _debugExpanded;

        static MainPage()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "org.comroid");
            var conf = Path.Combine(dir, "dirLinker.json");
            ConfigFile = new FileInfo(conf);
            var dirInf = new DirectoryInfo(dir);
            Debug.WriteLine(dir);
            Debug.WriteLine(dirInf);
            if (!dirInf.Exists)
                dirInf.Create();
        }

        public Configuration Config { get; private set; }

        public MainPage()
        {
            DataContext = this;
            InitializeComponent();
            SetWindowSize(840, 600);

            try
            {
                LoadConfig();
            } catch (Exception e)
            {
                WriteLine("Could not load configuration: " + e);
            }
        }

        internal void WriteLine(object line)
        {
            Debug.WriteLine("Debug Line: " + line);
            DebugOutput.Text += '\n' + line.ToString();
        }

        private void SetWindowSize(int width, int height)
        {
            ApplicationView.PreferredLaunchViewSize = new Size(width, height);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
        }

        private void ApplyConfig()
        {
            if (Config.ConfigVersion != 1)
                throw new InvalidDataException("Unknown configuration Version");
            foreach (var it in Config.LinkDirectories)
            {
                var parentDir = it.Dir;

                if (!parentDir.Exists)
                    continue; // todo: log skipped element

                foreach (var blob in it.Links)
                {
                    var linkPath = Path.Combine(parentDir.FullName, blob.LinkName);
                    var targetDir = new DirectoryInfo(blob.TargetDirectory);

                    if (!targetDir.Exists)
                        continue; // todo: log skipped element

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
                throw new InvalidOperationException("Link parent directory is missing");
            if (link.Exists)
                throw new InvalidOperationException("Link target already exists");
            if (!targetDir.Exists)
                throw new InvalidOperationException("Link target directory is missing");

            var dirObj = Config.LinkDirectories.Find(it => it.Directory == linkDir.FullName)
                ?? new Configuration.LinkDir { Dir = linkDir, Links = new List<Configuration.LinkBlob>() };
            var linkObj = new Configuration.LinkBlob { LinkName = linkName, TargetDir = targetDir };
            dirObj.Links.Add(linkObj);
        }

        private void LoadConfig()
        {
            if (!ConfigFile.Exists)
            {
                Config = CreateDefaultConfig();
                SaveConfig();
            }
            else
            {
                string data = File.ReadAllText(ConfigFile.FullName);
                Config = JsonConvert.DeserializeObject<Configuration>(data);
            }

            if (Config == null)
                throw new InvalidDataException("Could not load configuration");
        }

        private void SaveConfig()
        {
            File.WriteAllText(ConfigFile.FullName, JsonConvert.SerializeObject(Config));
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
            } catch (Exception ex)
            {
                WriteLine("Could not apply Configuration: " + ex);
            }
        }
        
        private void Button_ExpandDebug(object sender, RoutedEventArgs e)
        {
            try
            {
                ToggleDebugExpanded();
            } catch (Exception ex)
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
                WriteLine("Could not toggle debug window: " + ex);
            }
        }
    }
}
