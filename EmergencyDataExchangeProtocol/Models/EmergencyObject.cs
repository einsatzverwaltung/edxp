using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using EmergencyDataExchangeProtocol.EmergencyObjects;
using System.Runtime.Serialization;
using EmergencyDataExchangeProtocol.EmergencyObjects.einsatzmittel;

namespace EmergencyDataExchangeProtocol.Models
{
    public class EmergencyObject
    {
        [JsonProperty("@uid"), BsonId()]
        public Guid? uid { get; set; }
        [JsonProperty("@header")]
        public EmergencyObjectHeader header { get; set; }

        public object data { get; set; }
    }
}
