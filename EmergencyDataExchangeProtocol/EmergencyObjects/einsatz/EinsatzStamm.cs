using EmergencyDataExchangeProtocol.EmergencyObjects.common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.EmergencyObjects.einsatz
{
    public class EinsatzStamm
    {
       public Address Einsatzort { get; set; }

    }
}
