using EmergencyDataExchangeProtocol.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.EmergencyObjects.common
{
    public class GeoPosition
    {
        /// <summary>
        /// WSG84 Koordinaten
        /// </summary>
        [Required]
        public GeoCoordinates Coordinates { get; set; }

        /// <summary>
        /// Zeitpunkt der aktuellen Position
        /// </summary>
        public DateTime? PositionTime { get; set; }

        /// <summary>
        /// Genaugikeit der Positionsdaten in Metern
        /// </summary>
        public int? Accuracy { get; set; }

        /// <summary>
        /// Aktuelle Richtung in Grad
        /// </summary>
        [Range(0, 359)]
        public int? Heading { get; set; }
        /// <summary>
        /// Aktuelle Geschwindigkeit in m/s
        /// </summary>
        [LowerBoundValidation(0.0)]
        public double? Speed { get; set; }
    }
}
