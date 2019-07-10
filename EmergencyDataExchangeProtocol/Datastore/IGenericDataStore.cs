using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Models.auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Datastore
{
    public interface IGenericDataStore
    {
        EmergencyObject GetObjectFromDatastore(Guid uid, EndpointIdentity endpoint);
        ActionResult<EmergencyObject> CreateObjectInDatastore(EmergencyObject data, EndpointIdentity identity);
    }
}
