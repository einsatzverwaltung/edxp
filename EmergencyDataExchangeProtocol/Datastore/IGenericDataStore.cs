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
        
        GetObjectResult GetObjectFromDatastore(Guid uid);
        CreateObjectResult CreateObjectInDatastore(EmergencyObject data);
        ActionResult<EmergencyObject> UpdateObjectInDatastore(EmergencyObject datay);

        EndpointIdentity GetEndpointIdentityByApiKey(string apiKey);

        void InitIdentity();
    }
}
