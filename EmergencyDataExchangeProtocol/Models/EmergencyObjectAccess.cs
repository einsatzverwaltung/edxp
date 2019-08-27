using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Models
{
    public class EmergenyObjectAccessContainer 
    {
        public string idPattern { get; set; }
        public List<EmergencyObjectAccess> acl { get; set; }

        public EmergenyObjectAccessContainer() { }

        public EmergenyObjectAccessContainer(string id)
        {
            idPattern = id;
        }

        public void AddAccessRight(string path, AccessLevelEnum level)
        {
            if (acl == null)
                acl = new List<EmergencyObjectAccess>();
            acl.Add(new EmergencyObjectAccess()
            {
                level = level,
                path = path
            });
        }
    }

    public class EmergencyObjectAccess
    {
        public string path { get; set; }
        public AccessLevelEnum level { get; set; }
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
