﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.EmergencyObjects.einsatzmittel
{
    public class EinsatzmittelStamm
    {
        [Required]
        public string rufname { get; set; }
        public string normbezeichnung { get; set; }
        public string kennzeichen { get; set; }
    }
}
