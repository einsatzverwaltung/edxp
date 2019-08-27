using EmergencyDataExchangeProtocol.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.EmergencyObjects.einsatz
{
    public class Lagemeldung
    {
        /// <summary>
        /// Zeitpunkt, zu dem die Lagemeldung abgegeben wurde
        /// </summary>
        [Required]
        public DateTime zeit { get; set; }
        /// <summary>
        /// Meldungstext, der den Inhalt der Lagemeldung enthält
        /// </summary>
        [Required]
        public string meldung { get; set; }
        /// <summary>
        /// Quelle/Absender der Lagemeldung
        /// </summary>
        public string quelle { get; set; }
        /// <summary>
        /// Name des Abfassers der Lagemeldung (z.B. Disponent)
        /// </summary>
        public string abfasser { get; set; }
        /// <summary>
        /// Referenz auf ein bestehendes Einsatzmittel, dass die Lagemeldung abgegeben hat
        /// </summary>
        [ObjectReferenceValidation(Models.EmergencyObjectDataTypes.Einsatzmittel)]
        public Guid? referenzEinsatzmittel { get; set; }
    }
}
