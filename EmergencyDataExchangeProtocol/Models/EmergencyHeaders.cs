using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models
{
    public class EmergencyObjectHeader
    {
        public DateTime? created { get; set; }
        public Guid? createdBy { get; set; }
        public DateTime? lastUpdated { get; set; }
        public Guid? lastUpdatedBy { get; set; }
        public List<EmergenyObjectAccessContainer> Access { get; set; }
        public int timeToLive { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EmergencyObjectDataTypes dataType { get; set; }
    }

    public class EmergencyObjectCreateHeader
    {
        public Dictionary<string, EmergencyObjectAccess> Access { get; set; }
        public int timeToLive { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EmergencyObjectDataTypes dataType { get; set; }
    }

    public enum EmergencyObjectDataTypes
    {
        Einsatz = 0,
        Einsatzmittel = 1
    }
}
