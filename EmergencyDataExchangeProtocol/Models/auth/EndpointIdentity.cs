using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models.auth
{
    public class EndpointIdentity
    {
        [BsonId()]
        public Guid uid { get; set; }
        public string name { get; set; }
        public List<string> accessIdentity { get; set; }
        public List<string> apiKeys { get; set; }
    }
}
