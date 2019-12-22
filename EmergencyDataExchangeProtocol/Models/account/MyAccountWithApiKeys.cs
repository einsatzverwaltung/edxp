using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models.account
{
    public class MyAccountWithApiKeys : MyAccount
    {
        public List<string> apiKeys { get; set; }
    }
}
