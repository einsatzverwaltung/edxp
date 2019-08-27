using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.EmergencyObjects.einsatzmittel
{
    public class EinsatzmittelTelemetry
    {
        [Range(0,100)]
        public int? kraftstoffstand { get; set; }
    }
}
