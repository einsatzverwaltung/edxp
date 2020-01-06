using EmergencyDataExchangeProtocol.Models.auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Websocket
{
    public class WebsocketInfo
    {
        public WebSocket ws { get; set; }
        public EndpointIdentity endpoint { get; set; }
    }
}
