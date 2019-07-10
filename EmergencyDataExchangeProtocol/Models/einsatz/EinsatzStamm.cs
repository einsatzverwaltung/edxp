using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models.einsatz
{
    public class EinsatzStamm
    {
        public string stadt { get; set; }
        public string stadtteil { get; set; }
        public string postleitzahl { get; set; }
        public string strasse { get; set; }
        public string hausnummer { get; set; }
       
    }
}
