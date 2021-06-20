using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        static MainPage()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "comroid/dirLinker.json");
            ConfigFile = new FileInfo(path);
        }

        public Configuration Config { get; private set; }

        public MainPage()
        {
            DataContext = this;
            InitializeComponent();

            try
            {
                LoadConfig();
            } catch (Exception e)
            {
                Debug.WriteLine("Could not load configuration: " + e);
            }
        }

        internal void SubmitConfig()
        {
            if (Config.ConfigVersion != 1)
                throw new InvalidDataException("Unknown configuration Version");
            foreach (var it in Config.LinkDirectories)
            {
                var parentDir = it.Dir;

                foreach (var blob in it.Links)
                {
                    var linkDir = new DirectoryInfo(Path.Combine(it.Directory, blob.LinkName));
                    var targetDir = new DirectoryInfo(blob.TargetDirectory);
                }
            }
        }

        internal void LoadConfig()
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
        }

        internal void SaveConfig()
        {
            File.WriteAllText(ConfigFile.FullName, JsonConvert.SerializeObject(Config));
        }

        private Configuration CreateDefaultConfig()
        {
            var defaults = new Configuration();
            return defaults;
        }
    }
}
