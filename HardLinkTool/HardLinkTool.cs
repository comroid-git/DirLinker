using System;
using System.Diagnostics;
using System.Dynamic;
using System.Runtime.CompilerServices;
using DirLinkerConfig;
using Newtonsoft.Json;

namespace HardLinkTool
{
    public class HardLinkTool
    {
        public static Configuration Config { get; private set; }

        public static void Main(string[] args)
        {
            try
            {
                Run(args);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Internal Error: " + ex);
            }
            finally
            {
                Console.WriteLine("Press any Key to exit");
                Console.ReadKey();
            }
        }

        private static void Run(string[] args)
        {
            if (args[0].Equals("--applyconfig"))
            {
                Console.WriteLine("Config Mode");

                var data = new string[args.Length - 1];
                for (int i = 0; i < data.Length; i++)
                    data[i] = args[i + 1];
                var value = string.Join(' ', data);
                Console.WriteLine("Value: " + value);
                Config = JsonConvert.DeserializeObject<Configuration>(value);
                ApplyConfig();
                return;
            }

            // handle other arguments
        }

        private static void ApplyConfig()
        {
            Console.WriteLine("Applying Configuration! Link Directories: " + Config.LinkDirectories.Count);
        }
    }
}
