using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using DirLinkerConfig;
using Newtonsoft.Json;

namespace HardLinkTool
{
    public class CliOptions
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; } = false;

        [Option("data", Required = false, HelpText = "Set output to verbose messages.")]
        public string Data { get; set; } = string.Empty;
    }

    public class HardLinkTool
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CliOptions>(args)
                .WithParsed(Run)
                .WithNotParsed(ParserError);
        }

        private static void Run(CliOptions args)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(args.Data);
        }

        private static void ParserError(IEnumerable<Error> obj)
        {
            Console.WriteLine("Could not parse Arguments\n" + string.Join("\n", obj));
        }
    }
}
