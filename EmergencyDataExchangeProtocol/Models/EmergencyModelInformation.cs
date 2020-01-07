using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models
{
    public class EmergencyModelInformation
    {
        public static Dictionary<EmergencyObjectDataTypes, EmergencyModelInformation> RegisteredDataTypes = new Dictionary<EmergencyObjectDataTypes, EmergencyModelInformation>()
        {
            [EmergencyObjectDataTypes.Einsatz] = new EmergencyModelInformation() {
                Typ = typeof(EmergencyObjects.einsatz.Einsatz),
                MaximumTimeToLive = 60 * 24 * 14 /* 14 days */
            },
            [EmergencyObjectDataTypes.Einsatzmittel] = new EmergencyModelInformation()
            {
                Typ = typeof(EmergencyObjects.einsatzmittel.Einsatzmittel),
                MaximumTimeToLive = 0 /* forever */
            },
        };

        public Type Typ { get; set; }
        public int MaximumTimeToLive { get; set; }
    }
}
