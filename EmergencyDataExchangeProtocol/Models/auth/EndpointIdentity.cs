using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models.auth
{
    public class EndpointIdentity
    {
        public Guid uid { get; set; }
        public string name { get; set; }
        public List<string> accessIdentity { get; set; }
    }
}
