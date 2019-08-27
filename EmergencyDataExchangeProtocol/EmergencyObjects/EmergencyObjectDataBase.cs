using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.EmergencyObjects
{

    [KnownType(typeof(einsatz.Einsatz))]
    [KnownType(typeof(einsatzmittel.Einsatzmittel))]
    public class EmergencyObjectDataBase
    {
    }
}
