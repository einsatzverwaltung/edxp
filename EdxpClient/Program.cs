
using EdxpClient.edxp;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;

namespace EdxpClient
{
    [Command("edxp")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(
        typeof(Login),
        typeof(Account),
        typeof(ObjectCommand),
        typeof(WebSockCommand)
        )]
    class Program : CommandBase
    {
        static void Main(string[] args)
        {
            CommandLineApplication.Execute<Program>(args);

        }

       

        public int OnExecute(CommandLineApplication app)
        {
            // this shows help even if the --help option isn't specified
            app.ShowHelp();
            return 1;
        }


        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }


    public class CommandBase
    {
        [Option(CommandOptionType.NoValue, ShortName = "v", LongName = "verbose", Description = "Set to show Debug Information")]
        public bool Verbose { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "o", LongName = "output", Description = "Set the Output Format Type")]
        public OutputFormatType OutputFormat { get; set; }
    }

    public enum OutputFormatType
    {
        User = 0,
        YAML = 1,
        JSON = 2
    }
}
