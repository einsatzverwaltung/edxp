using EmergencyDataExchangeProtocol.Models.auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models.account
{
    public class MyAccount
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public bool isServerAdmin { get; set; }
        public List<string> accessIdentities { get; set; }
        public ContactDetails contact { get; set; }
    }
}
