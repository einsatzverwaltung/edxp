using EmergencyDataExchangeProtocol.Models;
using EmergencyDataExchangeProtocol.Models.auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace EmergencyDataExchangeProtocol.Datastore
{
    public class GenericDataStore : IGenericDataStore
    {
        MongoDbInterface db;
        AccessCheck acl = new AccessCheck();
        ILogger logger;



        public GenericDataStore(ILoggerFactory log)
        {
            logger = log.CreateLogger<GenericDataStore>();
            db = new MongoDbInterface();            
        }

        public void InitIdentity()
        {
            if (!db.HasIdentities())
            {
                /* Neue Identity anlegen */
                var newAdmin = new EndpointIdentity()
                {
                    accessIdentity = new List<string>() { "eu" },
                    apiKeys = new Dictionary<string, string>(),
                    name = "Default Admin Identity",
                    uid = Guid.NewGuid()
                };

                var key = new byte[48];
                using (var generator = RandomNumberGenerator.Create())
                    generator.GetBytes(key);
                string apiKey = Convert.ToBase64String(key);

                newAdmin.apiKeys.Add(apiKey, "Default");

                logger.LogInformation("There is no Identity - Created Admin with API Key " + apiKey);

                db.CreateIdentity(newAdmin);
            }
        }

        public EmergencyObject GetObjectFromDatastore(Guid uid, EndpointIdentity endpoint)
        {
            var emergencyObject = db.GetObjectFromDatastore(uid, "objects");

            bool isOwner = (emergencyObject.header.createdBy == endpoint.uid);

            bool canReadInGeneral = false;

            // TODO Berechtigungen prüfen, für Nicht-Besitzer

            if (isOwner)
                canReadInGeneral = true;

            if (!canReadInGeneral)
                return null;

            return emergencyObject;
        }

        public ActionResult<EmergencyObject> CreateObjectInDatastore(EmergencyObject data, EndpointIdentity identity)
        {
            
            
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

        public EndpointIdentity GetEndpointIdentityByApiKey(string apiKey)
        {
            return new EndpointIdentity();
        }
    }
}
