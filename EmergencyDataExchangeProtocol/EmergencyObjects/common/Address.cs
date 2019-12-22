using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.EmergencyObjects.common
{
    public class Address
    {        
        public string stadt { get; set; }
        public string stadtteil { get; set; }

        [StringLength(5, MinimumLength = 5, ErrorMessage = "Postleitzahl muss exakt 5 Ziffern lang sein")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Postleitzahl muss eine Zahl sein")]
        public string postleitzahl { get; set; }
        public string strasse { get; set; }
        public string hausnummer { get; set; }

        public GeoCoordinates koordinaten { get; set; }
    }
}
