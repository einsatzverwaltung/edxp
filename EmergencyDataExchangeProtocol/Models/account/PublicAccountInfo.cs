using EmergencyDataExchangeProtocol.Models.auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models.account
{
    public class PublicAccountInfo
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public List<string> accessKeys { get; set; }
        public ContactDetails contact { get; set; }
    }
}
