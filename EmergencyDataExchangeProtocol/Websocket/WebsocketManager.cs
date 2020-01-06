using EmergencyDataExchangeProtocol.Datastore;
using EmergencyDataExchangeProtocol.Models.auth;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Websocket
{
    public class WebsocketManager : IWebsocketManager
    {
        IGenericDataStore db;

        List<WebsocketInfo> activeWebsockets = new List<WebsocketInfo>();

        public WebsocketManager(IGenericDataStore data)
        {
            db = data;
        }

        public async Task OnWebsocketConnected(HttpContext context, WebSocket ws)
        {
            /* Check API Key */
            string[] pathSegments = context.Request.Path.ToString().Split('/');
            string apiKey = string.Empty;

            EndpointIdentity endpoint;
            
            if (context.Request.Query.ContainsKey("key"))
            {
                apiKey = context.Request.Query["key"];

                endpoint = db.GetEndpointIdentityByApiKey(apiKey);

                if (endpoint == null)
                {
                    /* API Key falsch! */                    
                    await ws.CloseAsync(WebSocketCloseStatus.PolicyViolation, "API Key invalid!", CancellationToken.None);
                    return;
                }
            }
            else
            {
                await ws.CloseAsync(WebSocketCloseStatus.PolicyViolation, "API Key invalid!", CancellationToken.None);
                return;
            }

            activeWebsockets.Add(new WebsocketInfo()
            {
                ws = ws,
                endpoint = endpoint
            });

            /* Send Init Commands, if Timestamp is given */
            if (context.Request.Query.ContainsKey("since"))
            {
                string sinceTS = context.Request.Query["since"];
                DateTime sinceTime;

                if (DateTime.TryParse(sinceTS, out sinceTime))
                {
                    /* Load Objects from Database which have been modified since given Timestamp */

                }
                else
                {
                    await ws.CloseAsync(WebSocketCloseStatus.ProtocolError, "Since Timestamp invalid!", CancellationToken.None);
                    return;
                }

            }
        }
    }

    interface IWebsocketManager
    {
        Task OnWebsocketConnected(HttpContext context, WebSocket ws);
    }
}
