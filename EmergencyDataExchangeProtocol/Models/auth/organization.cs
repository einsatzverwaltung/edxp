using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models.auth
{
    public class organization
    {
        public Guid uid { get; set; }
        public string name { get; set; }

        public string identifier { get; set; }
    }
}
