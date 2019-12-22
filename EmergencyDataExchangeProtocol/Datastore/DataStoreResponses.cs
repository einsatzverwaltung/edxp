using EmergencyDataExchangeProtocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Datastore
{
    public class GetObjectResult
    {
        public EmergencyObject data { get; set; }
    }

    public class DeleteObjectResult
    {
        public bool deleted { get; set; }
    }

    public class CreateObjectResult
    {
        public object createdObject { get; set; }
        public WriteResult writeResult { get; set; }
    }

    public enum WriteResult
    {
        OK = 0,
        Duplicated = 1,
        ServerError = 2
    }

}
