using EdxpClient.tools;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EdxpClient
{
    [Command(Name = "login", Description = "Login on Server with given API Key")]
    class Login : CommandBase
    {
        [Argument(0, Description = "API Key which should be used to call methods on EDXP Server", Name = "API Key")]
        public string ApiKey { get; set; }

        [Option("--server", Description = "Server API URL which should be used")]
        public string Server { get; set; }

        public async Task OnExecute(IConsole console)
        {

            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\.edxp";
            LocalSettings settings = null;

            string server = "https://edxp.bosmesh.org";
            string apiKey = "";

            if (server == null)
            {
                server = "https://edxp.bosmesh.org";
            }

            if (File.Exists(path))
            {
                var jsonRead = File.ReadAllText(path);
                settings = JsonConvert.DeserializeObject<LocalSettings>(jsonRead);
                server = settings.baseUrl;
                apiKey = settings.apiKey;
            }

            if (Server != null)
            {
                server = Server;
            }

            if (ApiKey != null)
            {
                apiKey = ApiKey;
            }

            var c = ClientFactory.Create(Verbose, server, apiKey);

            var res = await c.GetAccountInfoAsync();

            if (settings == null)
            {
                settings = new LocalSettings();
            }

            settings.apiKey = apiKey;
            settings.baseUrl = server;

            var json = JsonConvert.SerializeObject(settings);

            File.WriteAllText(path, json);

            Console.Error.WriteLine("Login successful!");

            Helper.DumpObjectToOutput(res, OutputFormat);
        }
    }
}
