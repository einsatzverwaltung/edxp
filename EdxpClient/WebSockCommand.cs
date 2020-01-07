using EdxpClient.edxp;
using EdxpClient.tools;
using EdxpClient.ws;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EdxpClient
{
    [Command(Name = "ws", Description = "Websocket Connection for Live Updates")]
    public class WebSockCommand : CommandBase
    {
        [Argument(0, Description = "Timestamp to retrieve all modified objects", Name = "ModifiedSince")]
        public DateTime? ModifiedSince { get; set; }

        public async Task<int> OnExecute(CommandLineApplication app)
        {
            LocalSettings settings;

            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (File.Exists(path + @"\.edxp"))
            {
                var text = File.ReadAllText(path + @"\.edxp");
                settings = JsonConvert.DeserializeObject<LocalSettings>(text);
            }
            else
            {
                throw new Exception("You are not logged in! Please login with \"edxp login <ApiKey>\".");
            }

            ClientWebSocket client;

            client = new ClientWebSocket();

            string getParameterSince = string.Empty;

            if (ModifiedSince.HasValue)
            {
                getParameterSince = "&since=" + ModifiedSince.Value.ToString();
            }

            string baseUrl = settings.baseUrl;

            baseUrl = baseUrl.Replace("https://", "wss://");
            baseUrl = baseUrl.Replace("http://", "ws://");
            baseUrl = baseUrl.TrimEnd('/');

            client.Options.Proxy = WebRequest.DefaultWebProxy;
            client.Options.UseDefaultCredentials = true;

            Console.WriteLine("Connecting to Websocket Live Update Server...");

            try
            {
                string uri = baseUrl + "/ws?key=" + WebUtility.UrlEncode(settings.apiKey) + getParameterSince;
                await client.ConnectAsync(new Uri(uri), CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 1;
            }

            Console.WriteLine("Connected");

            var buffer = ClientWebSocket.CreateClientBuffer(4 * 1024, 4 * 1024);

            WebSocketReceiveResult result;

            do
            {

                using (var ms = new MemoryStream())
                {
                    do
                    {
                        result = await client.ReceiveAsync(buffer, CancellationToken.None);
                        ms.Write(buffer.Array, buffer.Offset, result.Count);

                        if (result.EndOfMessage)
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                using (var reader = new StreamReader(ms, Encoding.UTF8))
                                {
                                    var json = reader.ReadToEnd();
                                    var data = JsonConvert.DeserializeObject<EmergencyObjectMessage>(json);
                                    Helper.DumpObjectToOutput(data, OutputFormat);
                                    Console.WriteLine("");
                                }
                            }
                        }
                    } while (!result.EndOfMessage);
                }
            } while (!result.CloseStatus.HasValue);

            return 0;
        }
    }
}
