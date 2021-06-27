using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using DirLinkerConfig;
using Newtonsoft.Json;
using SymbolicLinkSupport;

namespace HardLinkTool
{
    public class HardLinkTool
    {
        public static readonly string DataDir;
        public static readonly string ConfigFile;

        static HardLinkTool()
        {
            DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "org.comroid");
            ConfigFile = Path.Combine(DataDir, "dirLinker.json");
            Directory.CreateDirectory(DataDir);
        }

        public static Configuration Config { get; private set; }

        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Arguments: " + string.Join(' ', args));
                Run(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Internal Error: " + ex);
            }
            finally
            {
                Console.WriteLine("Press any Key to exit");
                Console.ReadKey();
            }
        }

        private static void Run(string[] args)
        {
            if (args[0].Equals(DirLinkerInfo.ApplyConfigArgument))
            {
                LoadConfig();
                ApplyConfig();
                return;
            }

            // handle other arguments
        }

        private static void ApplyConfig()
        {
            Console.WriteLine("Applying Configuration! Link Directories: " + Config.LinkDirectories.Count);


            if (Config.ConfigVersion != 1)
                throw new InvalidDataException("Unknown configuration Version");
            foreach (var it in Config.LinkDirectories)
            {
                var parentDir = it.Dir;

                if (!parentDir.Exists)
                {
                    Console.WriteLine($"Missing link base directory: {parentDir}; skipping entry");
                    continue;
                }

                foreach (var blob in it.Links)
                {
                    var linkPath = Path.Combine(parentDir.FullName, blob.LinkName) + Path.DirectorySeparatorChar;
                    var targetDir = new DirectoryInfo(blob.TargetDirectory);

                    if (!targetDir.Exists)
                    {
                        Console.WriteLine($"Missing link target directory: {targetDir}; skipping entry");
                        continue;
                    }
                    if (!Directory.Exists(linkPath))
                    {
                        Console.WriteLine($"Link directory already exists: {linkPath}; skipping entry");
                        continue;
                    }

                    targetDir.CreateSymbolicLink(linkPath);
                }
            }
        }

        private static void LoadConfig()
        {
            if (File.Exists(ConfigFile))
            {
                string data = File.ReadAllText(ConfigFile);
                Config = JsonConvert.DeserializeObject<Configuration>(data) ?? CreateDefaultConfig();
            }
            else Config = CreateDefaultConfig();

            if (Config == null)
                throw new InvalidDataException("Could not load configuration");
        }

        private static void SaveConfig()
        {
            //ApplyConfigFromUI();
            var data = JsonConvert.SerializeObject(Config);
            File.WriteAllText(ConfigFile, data);
            Debug.WriteLine("Config was saved. Data: " + data);
        }

        private static Configuration CreateDefaultConfig()
        {
            var defaults = new Configuration();
            return defaults;
        }
    }
}