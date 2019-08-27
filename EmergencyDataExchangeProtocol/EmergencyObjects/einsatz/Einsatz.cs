using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.EmergencyObjects.einsatz
{
    public class Einsatz
    {
        [Required]
        public EinsatzStamm stamm { get; set; }
    }
}
