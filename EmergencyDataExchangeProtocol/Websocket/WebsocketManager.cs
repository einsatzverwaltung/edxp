using EmergencyDataExchangeProtocol.Controllers.v1;
using EmergencyDataExchangeProtocol.Datastore;
using EmergencyDataExchangeProtocol.Models.auth;
using EmergencyDataExchangeProtocol.Service;
using EmergencyDataExchangeProtocol.Websocket.Message;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
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

        public async Task PropagateChange(EmergencyObjectMessage data)
        {
            data.header.created = null;
            data.header.createdBy = null;
            data.header.lastUpdated = null;
            data.header.lastUpdatedBy = null;            

            var jsonData = JsonConvert.SerializeObject(data);
            var jsonDataBinary = Encoding.UTF8.GetBytes(jsonData);

            Parallel.ForEach(activeWebsockets, async (ws) =>
            {
                if (ws.ws.State == WebSocketState.Open)
                {
                    // TODO, Prüfen, ob Gegenstelle dies überhaupt empfangen darf
                    await ws.ws.SendAsync(new ArraySegment<byte>(jsonDataBinary), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            });
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

            var buffer = new byte[1024 * 4];

            /* Send Init Commands, if Timestamp is given */
            if (context.Request.Query.ContainsKey("since"))
            {
                string sinceTS = context.Request.Query["since"];
                DateTime sinceTime;

                if (DateTime.TryParse(sinceTS, out sinceTime))
                {
                    /* Load Objects from Database which have been modified since given Timestamp */
                    var objService = (ObjectService)context.RequestServices.GetService(typeof(ObjectService));

                    var modObjects = objService.GetModifiedObjects(sinceTime, endpoint);

                    foreach (var emergencyObject in modObjects)
                    {
                        var msg = JsonConvert.SerializeObject(new EmergencyObjectMessage()
                        {
                            data = emergencyObject.data,
                            header = emergencyObject.header,
                            uid = emergencyObject.uid,
                            messageTrigger = MessageSentTrigger.Init
                        });

                        var msgBytes = Encoding.UTF8.GetBytes(msg);

                        await ws.SendAsync(new ArraySegment<byte>(msgBytes, 0, msgBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else
                {
                    await ws.CloseAsync(WebSocketCloseStatus.ProtocolError, "Since Timestamp invalid!", CancellationToken.None);
                    return;
                }

            }

            WebSocketReceiveResult result;

            do
            {
                result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            } while (!result.CloseStatus.HasValue);

            await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }

    public interface IWebsocketManager
    {
        Task OnWebsocketConnected(HttpContext context, WebSocket ws);
        Task PropagateChange(EmergencyObjectMessage data);
    }
}
