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
        List<EmergencyObject> GetObjectsFromDatastoreSince(DateTime since);
        GetObjectResult GetObjectFromDatastore(Guid uid);
        CreateObjectResult CreateObjectInDatastore(EmergencyObject data);
        DeleteObjectResult DeleteObjectInDatastore(Guid uid);
        ActionResult<EmergencyObject> UpdateObjectInDatastore(EmergencyObject datay);


        EndpointIdentity GetEndpointIdentityByApiKey(string apiKey);
        EndpointIdentity GetEndpointIdentityByGuid(Guid id);
        EndpointIdentity CreateNewEndpoint(string name, bool isServerAdmin, List<string> accessIdentifiers, ContactDetails contact);
        void DeleteEndpoint(Guid id);
        bool UpdateEndpointInDatastore(EndpointIdentity endpoint);
        IEnumerable<EndpointIdentity> GetEndpointIdentities();

        void InitIdentity();
    }
}
