using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.EmergencyObjects.einsatzmittel
{
    public class Einsatzmittel : EmergencyObjectDataBase
    {
        [Required]
        public EinsatzmittelStamm stamm { get; set; }
        public EinsatzmittelStatus status { get; set; }

        public EinsatzmittelTelemetry telemetry { get; set; }
    }
}
