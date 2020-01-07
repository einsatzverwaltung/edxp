using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Websocket.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Websocket
{
    public class ObjectChangeTracker
    {
        IWebsocketManager ws;

        public ObjectChangeTracker(IWebsocketManager wsManager)
        {
            ws = wsManager;
        }

        public void ObjectChanged(Guid uid, MessageSentTrigger trigger, EmergencyObject edxo, object subData, string subPath = null)
        {
            var eventMsg = new EmergencyObjectMessage()
            {
                uid = uid,
                header = edxo.header,
                messageTrigger = trigger,
            };

            if (subPath != null)
            {
                eventMsg.isParted = true;
                eventMsg.partPath = subPath;
                eventMsg.data = subData;
            }
            else
            {
                eventMsg.isParted = false;                
                eventMsg.data = edxo.data;
            }

            ws.PropagateChange(eventMsg);
        }
    }
}
