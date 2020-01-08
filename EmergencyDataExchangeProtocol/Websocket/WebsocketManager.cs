using EmergencyDataExchangeProtocol.Controllers.v1;
using EmergencyDataExchangeProtocol.Datastore;
using EmergencyDataExchangeProtocol.Models;
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
        ObjectService objectService;

        List<WebsocketInfo> activeWebsockets = new List<WebsocketInfo>();

        public WebsocketManager(IGenericDataStore data, ObjectService os)
        {
            db = data;
            objectService = os;
        }

        public async Task PropagateChange(EmergencyObjectMessage data)
        {
            var access = data.header.Access;
            var owner = data.header.createdBy;                       

            Parallel.ForEach(activeWebsockets, async (ws) =>
            {
                if (ws.ws.State == WebSocketState.Open)
                {                    
                    if (ws.endpoint.uid == owner)
                    {
                        /* Owner will receive all data regardless of access control list */
                        var sendObject = data.Copy<EmergencyObjectMessage>();
                        await sendObjectToWebsocket(ws.ws, sendObject);
                    }
                    else
                    {
                        /* Get allowed Paths for this user and reduce object to allowed paths */
                        var paths = objectService.GetAccessPaths(Models.AccessLevelEnum.Read, data, ws.endpoint);
                        if (paths != null && paths.Count > 0)
                        {
                            var sendObject = data.Copy<EmergencyObjectMessage>();
                            sendObject.data = objectService.RemoveUnallowedPaths<object>(paths, sendObject.data, data.partPath);
                            await sendObjectToWebsocket(ws.ws, sendObject);
                        }
                    }
                }
            });
        }

        private async Task sendObjectToWebsocket(WebSocket ws, EmergencyObjectMessage sendObject)
        {
            removeHeadersNotToBeSent(sendObject);

            var jsonData = JsonConvert.SerializeObject(sendObject);
            var jsonDataBinary = Encoding.UTF8.GetBytes(jsonData);

            await ws.SendAsync(new ArraySegment<byte>(jsonDataBinary), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private void removeHeadersNotToBeSent(EmergencyObjectMessage sendObject)
        {
            sendObject.header.created = null;
            sendObject.header.createdBy = null;
            sendObject.header.lastUpdated = null;
            sendObject.header.lastUpdatedBy = null;
            sendObject.header.Access = null;
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
