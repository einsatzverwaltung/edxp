using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models
{
    public class EmergencyObjectAccess : Dictionary<string, AccessLevelEnum>
    {

    }

    public enum AccessLevelEnum
    {
        None    = 0x0000,
        Read    = 0x0001,
        Write   = 0x0002,
        Grant   = 0x0004,
        Owner   = 0x8000
    }
}
