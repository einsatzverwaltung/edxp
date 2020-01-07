using EdxpClient.edxp;
using EdxpClient.tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace EdxpClient
{
    static class ClientFactory
    {
        /// <summary>
        /// Create Client Instance with neccessary information
        /// </summary>
        /// <param name="verbose"></param>
        /// <param name="baseUrl"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static Client Create(bool verbose, string baseUrl, string apiKey)
        {
            var httpClientHandler = new HttpClientHandler
            {
                UseProxy = true,
                DefaultProxyCredentials = CredentialCache.DefaultCredentials
            };

            HttpClient http = new HttpClient(httpClientHandler);

            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            return new edxp.Client(baseUrl, http);
        }

        /// <summary>
        /// Create client with default logged in credentials
        /// </summary>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static Client Create(bool verbose)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (File.Exists(path + @"\.edxp"))
            {
                var text = File.ReadAllText(path + @"\.edxp");

                var json = JsonConvert.DeserializeObject<LocalSettings>(text);

                return Create(verbose, json.baseUrl, json.apiKey);
            }
            else
            {
                throw new Exception("You are not logged in! Please login with \"edxp login <ApiKey>\".");
            }
        }


    }
}
