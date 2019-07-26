using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Models.auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Datastore
{
    public class GenericDataStore : IGenericDataStore
    {
        MongoDbInterface db;
        AccessCheck acl = new AccessCheck();

        Dictionary<EmergencyObjectDataTypes, Type> registeredDataTypes = new Dictionary<EmergencyObjectDataTypes, Type>()
        {
            [EmergencyObjectDataTypes.Einsatz] = typeof(Models.einsatz.Einsatz),
            [EmergencyObjectDataTypes.Einsatzmittel] = typeof(Models.einsatzmittel.Einsatzmittel)
        };

        public GenericDataStore()
        {
            db = new MongoDbInterface();
        }

        public EmergencyObject GetObjectFromDatastore(Guid uid, EndpointIdentity endpoint)
        {
            var emergencyObject = db.GetObjectFromDatastore(uid, "objects");

            bool isOwner = (emergencyObject.header.createdBy == endpoint.uid);

            bool canReadInGeneral = false;

            if (isOwner)
                canReadInGeneral = true;

            if (!canReadInGeneral)
                return null;

            return emergencyObject;
        }

        public ActionResult<EmergencyObject> CreateObjectInDatastore(EmergencyObject data, EndpointIdentity identity)
        {
            AccessCheck ac = new AccessCheck();

            if (data == null)
            {
                return new NoContentResult();
            }

            if (!data.uid.HasValue)
            {
                data.uid = Guid.NewGuid();
            }

            if (data.header == null)
            {
                data.header = new EmergencyObjectHeader();
            }
            data.header.created = DateTime.UtcNow;
            data.header.createdBy = identity.uid; // TODO
            if (data.header.timeToLive < 120 && data.header.timeToLive != 0)
            {
                data.header.timeToLive = 120;
            }
            if (data.header.timeToLive > 60 * 24 * 30)
            {
                data.header.timeToLive = 60 * 24 * 30;
            }

            if (!registeredDataTypes.ContainsKey(data.header.dataType))
            {
                return new BadRequestResult();
            }

            var dataType = registeredDataTypes[data.header.dataType];
            var body = data.data.ToObject(dataType);
            
            var res = db.CreateObjectInDatastore(data, "objects");
            if (res == MongoDbInterface.WriteResult.OK)
            {
                return new CreatedResult("/object/" + data.uid.Value.ToString(), data);
            }
            else if (res == MongoDbInterface.WriteResult.Duplicated)
            {
                return new ConflictResult();
            }
            else
            {
                return new StatusCodeResult(500);
            }
        }
    }
}
