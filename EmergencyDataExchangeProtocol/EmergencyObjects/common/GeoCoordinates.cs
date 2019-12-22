using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.EmergencyObjects.common
{
    public class GeoCoordinates
    {

        /// <summary>
        /// WGS84 Latitude der aktuellen Position
        /// </summary>
        [Required]
        [Range(-90.0, 90.0)]
        public double Latitude { get; set; }
        /// <summary>
        /// WGS84 Longitude der aktuellen Position
        /// </summary>
        [Required]
        [Range(-180.0, 180.0)]
        public double Longitude { get; set; }

        /// <summary>
        /// Altitude (Höhe) in Metern
        /// </summary>
        public double? Altitude { get; set; }
    }
}
