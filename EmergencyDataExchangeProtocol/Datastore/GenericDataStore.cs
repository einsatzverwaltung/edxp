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
                    apiKeys = new List<string>(),
                    name = "Default Admin Identity",
                    uid = Guid.NewGuid(),
                    isServerAdmin = true,
                    contact = new ContactDetails()
                    {
                        contactMail = "admin@example.org",
                        contactName = "Administrator"
                    }
                };

                var key = new byte[48];
                using (var generator = RandomNumberGenerator.Create())
                    generator.GetBytes(key);
                string apiKey = Convert.ToBase64String(key);

                newAdmin.apiKeys.Add(apiKey);

                logger.LogInformation("There is no Identity - Created Admin with API Key " + apiKey);

                db.CreateIdentity(newAdmin);            
            }
        }

        public IEnumerable<EndpointIdentity> GetEndpointIdentities()
        {
            return db.GetAllEndpointsFromDatastore();
        }
        public EndpointIdentity CreateNewEndpoint(string name, bool isServerAdmin, List<string> accessIdentifiers, ContactDetails contact)
        {
            var newId = new EndpointIdentity()
            {
                accessIdentity = accessIdentifiers,
                apiKeys = new List<string>(),
                name = name,
                uid = Guid.NewGuid(),
                isServerAdmin = isServerAdmin,
                contact = contact
            };


            var key = new byte[48];
            using (var generator = RandomNumberGenerator.Create())
                generator.GetBytes(key);
            string apiKey = Convert.ToBase64String(key);

            newId.apiKeys.Add(apiKey);

            db.CreateIdentity(newId);

            return newId;
        }

        public bool UpdateEndpointInDatastore(EndpointIdentity endpoint)
        {
            var res = db.UpdateIdentity(endpoint);
            if (res == WriteResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DeleteEndpoint(Guid id)
        {            
            db.DeleteIdentity(id);
        }

        public GetObjectResult GetObjectFromDatastore(Guid uid)
        {
            var emergencyObject = db.GetObjectFromDatastore(uid, "objects");

            return new GetObjectResult()
            {
                data = emergencyObject
            };
        }

        public CreateObjectResult CreateObjectInDatastore(EmergencyObject data)
        {
            CreateObjectResult result = new CreateObjectResult();

            result.writeResult = db.CreateObjectInDatastore(data, "objects");
            result.createdObject = data;

            return result;

        }

        public EndpointIdentity GetEndpointIdentityByGuid(Guid id)
        {
            return db.GetIdentityById(id);
        }

        public EndpointIdentity GetEndpointIdentityByApiKey(string apiKey)
        {
            return db.GetIdentityByApiKey(apiKey);
        }

        public DeleteObjectResult DeleteObjectInDatastore(Guid uid)
        {
            return db.DeleteObjectInDatastore(uid, "objects");
        }

        public ActionResult<EmergencyObject> UpdateObjectInDatastore(EmergencyObject data)
        {

            var res = db.UpdateObjectInDatastore(data, "objects");
            if (res == WriteResult.OK)
            {
                return new CreatedResult("/object/" + data.uid.Value.ToString(), data);
            }
            else if (res == WriteResult.Duplicated)
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
