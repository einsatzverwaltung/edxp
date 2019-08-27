using EmergencyDataExchangeProtocol.EmergencyObjects.common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.EmergencyObjects.einsatzmittel
{
    public class EinsatzmittelStatus
    {
        [Range(1,8)]
        public int? status { get; set; }

        public GeoPosition position { get; set; }
    }
}
